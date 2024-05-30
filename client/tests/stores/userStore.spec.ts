import { jwtDecode } from 'jwt-decode';
import { createPinia } from 'pinia';
import { Mock, afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { createApp } from 'vue';
import { AuthServiceFactoryKey, IAuthService } from '../../src/services/authService';
import { ClientFactoryKey, IClient } from '../../src/services/client';
import { USER_KEY, useUserStore } from '../../src/stores/userStore';

vi.mock('jwt-decode', async importOriginal => {
  const actual = await importOriginal<object>();
  return {
    ...actual,
    jwtDecode: vi.fn(),
  };
});

const mocks = vi.hoisted(() => {
  return {
    jwtDecode: vi.fn(),
  };
});

vi.mock('jwtDecode', () => {
  return {
    jwtDecode: mocks.jwtDecode,
  };
});

describe('userStore', () => {
  const localStorageMock = {
    getItem: vi.fn(),
    setItem: vi.fn(),
    clear: vi.fn(),
    removeItem: vi.fn(),
    key: vi.fn(),
    length: 0,
  };

  const originalStorage = global.localStorage;

  const clientMock: IClient = {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  };

  const authServiceMock: IAuthService = {
    login: vi.fn(),
    logout: vi.fn(),
    refreshToken: vi.fn(),
    register: vi.fn(),
    verifyAccount: vi.fn(),
    resendVerificationEmail: vi.fn(),
  };

  beforeEach(() => {
    const app = createApp({});
    const pinia = createPinia();

    app.provide(ClientFactoryKey, {
      create: vi.fn(() => clientMock),
    });

    app.provide(AuthServiceFactoryKey, {
      create: vi.fn(() => authServiceMock),
    });

    app.use(pinia);

    global.localStorage = localStorageMock;
  });

  afterEach(() => {
    vi.restoreAllMocks();
    global.localStorage = originalStorage;
  });

  it('should return null if user is not stored in local storage', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { user } = useUserStore();

    expect(user).toBe(null);
  });

  it('should return user if user is stored in local storage', () => {
    const fakeUser = {
      id: 'test',
      expiresAtInSeconds: 123,
      token: 'token',
    };

    localStorageMock.getItem.mockReturnValue(JSON.stringify(fakeUser));

    const { user } = useUserStore();

    expect(user).toEqual(fakeUser);
  });

  it('should provide a logUserIn action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { logUserIn } = useUserStore();
    expect(logUserIn).toBeInstanceOf(Function);
  });

  it('should provide a logUserOut action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { logUserOut } = useUserStore();
    expect(logUserOut).toBeInstanceOf(Function);
  });

  it('should provide a refreshAccessToken action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { refreshAccessToken } = useUserStore();
    expect(refreshAccessToken).toBeInstanceOf(Function);
  });

  it('should provide an updateSidebarState action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { updateSidebarState } = useUserStore();
    expect(updateSidebarState).toBeInstanceOf(Function);
  });

  describe('logUserIn', () => {
    it('should store user in local storage and set user', () => {
      localStorageMock.getItem.mockReturnValueOnce(null);
      const store = useUserStore();
      const payload = { sub: 'test', exp: 123, token: 'token' };
      vi.mocked(jwtDecode).mockReturnValue(payload);

      const fakeUser = {
        id: payload.sub,
        expiresAtInSeconds: payload.exp,
        token: payload.token,
        expanded: true,
      };

      store.logUserIn(payload.token);

      expect(localStorageMock.setItem).toHaveBeenCalledWith(USER_KEY, JSON.stringify(fakeUser));
      expect(store.user).toEqual(fakeUser);
    });
  });

  describe('logUserOut', () => {
    it('should remove user from local storage and set user to null', () => {
      localStorageMock.getItem.mockReturnValue(
        JSON.stringify({
          id: 'test',
          expiresAtInSeconds: 123,
          token: 'token',
        })
      );

      const store = useUserStore();

      store.logUserOut();

      expect(localStorageMock.removeItem).toHaveBeenCalledWith(USER_KEY);
      expect(store.user).toBe(null);
    });
  });

  describe('refreshAccessToken', () => {
    it('should log user out if user is not logged in and return 401 response', async () => {
      localStorageMock.getItem.mockReturnValue(null);

      const store = useUserStore();

      const response = await store.refreshAccessToken(new Request('https://test.com'));

      expect(store.user).toBe(null);
      expect(response.status).toBe(401);
    });

    it('should log user out if refreshing token fails and return 401 response', async () => {
      localStorageMock.getItem.mockReturnValue(
        JSON.stringify({
          id: 'test',
          expiresAtInSeconds: 123,
          token: 'token',
        })
      );

      (authServiceMock.refreshToken as Mock).mockResolvedValue({ err: true });

      const store = useUserStore();

      const response = await store.refreshAccessToken(new Request('https://test.com'));

      expect(store.user).toBe(null);
      expect(response.status).toBe(401);
    });

    it('should log user in with new token and return 200 response', async () => {
      localStorageMock.getItem.mockReturnValue(
        JSON.stringify({
          id: 'test',
          expiresAtInSeconds: 123,
          token: 'token',
        })
      );

      (authServiceMock.refreshToken as Mock).mockResolvedValue({
        err: false,
        val: { accessToken: 'token' },
      });

      const payload = { sub: 'test', exp: 123, token: 'token' };
      vi.mocked(jwtDecode).mockReturnValue(payload);

      const user = {
        id: payload.sub,
        expiresAtInSeconds: payload.exp,
        token: payload.token,
        expanded: true,
      };

      global.fetch = vi.fn().mockResolvedValue(new Response());

      const store = useUserStore();

      const response = await store.refreshAccessToken(new Request('https://test.com'));

      expect(store.user).toEqual(user);
      expect(response.status).toBe(200);
    });
  });

  describe('updateSidebarState', () => {
    it('should do nothing if user is not logged in', () => {
      localStorageMock.getItem.mockReturnValue(null);

      const store = useUserStore();

      store.updateSidebarState(true);

      expect(localStorageMock.setItem).not.toHaveBeenCalled();
    });

    it('should update user in local storage', () => {
      const user = {
        id: 'test',
        expiresAtInSeconds: 123,
        token: 'token',
        expanded: true,
      };

      const updatedUser = { ...user, expanded: false };

      localStorageMock.getItem.mockReturnValue(JSON.stringify(user));

      const store = useUserStore();

      store.updateSidebarState(false);

      expect(localStorageMock.setItem).toHaveBeenCalledWith(USER_KEY, JSON.stringify(updatedUser));
    });
  });
});

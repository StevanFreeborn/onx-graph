import { jwtDecode } from 'jwt-decode';
import { createPinia, setActivePinia } from 'pinia';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { createApp } from 'vue';
import { useUserStore } from '../src/stores/userStore';
import { AuthServiceFactoryKey, IAuthService } from './../src/services/authService';
import { ClientFactoryKey, IClient } from './../src/services/client';
import { USER_KEY } from './../src/stores/userStore';

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
    app.provide;
    app.use(pinia);

    setActivePinia(pinia);

    global.localStorage = localStorageMock;
  });

  afterEach(() => {
    vi.restoreAllMocks();
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

  it('it should provide a logUserIn action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { logUserIn } = useUserStore();
    expect(logUserIn).toBeInstanceOf(Function);
  });

  it('it should provide a logUserOut action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { logUserOut } = useUserStore();
    expect(logUserOut).toBeInstanceOf(Function);
  });

  it('it should provide a refreshAccessToken action', () => {
    localStorageMock.getItem.mockReturnValue(null);

    const { refreshAccessToken } = useUserStore();
    expect(refreshAccessToken).toBeInstanceOf(Function);
  });

  describe('logUserIn', () => {
    it('should store user in local storage and set user', () => {
      localStorageMock.getItem.mockReturnValueOnce(null);
      const { logUserIn, user } = useUserStore();
      const payload = { sub: 'test', exp: 123, token: 'token' };
      vi.mocked(jwtDecode).mockReturnValue(payload);

      const fakeUser = {
        id: payload.sub,
        expiresAtInSeconds: payload.exp,
        token: payload.token,
      };

      logUserIn(payload.token);

      expect(localStorageMock.setItem).toHaveBeenCalledWith(USER_KEY, JSON.stringify(fakeUser));
      expect(user).toEqual(fakeUser);
    });
  });

  // describe('logUserOut', () => {
  //   it('should remove user from local storage and set user to null', () => {
  //     localStorageMock.getItem.mockReturnValue(
  //       JSON.stringify({
  //         id: 'test',
  //         expiresAtInSeconds: 123,
  //         token: 'token',
  //       })
  //     );

  //     const { logUserOut, user } = useUserStore();

  //     logUserOut();

  //     expect(localStorageMock.removeItem).toHaveBeenCalledWith(USER_KEY);
  //     expect(user).toBe(null);
  //   });
  // });
});

import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import NavigationSidebar from '../../src/components/NavigationSidebar.vue';
import { AuthServiceFactoryKey } from '../../src/services/authService';
import { UsersServiceFactoryKey } from '../../src/services/usersService';
import { useUserStore } from '../../src/stores/userStore';
import { customRender } from '../testUtils';

vi.mock('vue-router', async importOriginal => {
  const actual = await importOriginal<typeof import('vue-router')>();
  const mock = vi.fn();
  return {
    ...actual,
    useRouter: () => ({ push: mock }),
  };
});

describe('NavigationSidebar', () => {
  const localStorageMock = {
    getItem: vi.fn(),
    setItem: vi.fn(),
    clear: vi.fn(),
    removeItem: vi.fn(),
    key: vi.fn(),
    length: 0,
  };

  const originalStorage = global.localStorage;

  const mockUsersService = {
    getUser: () => ({ err: false, val: { username: 'test-username' } }),
  };

  const defaultProvide = {
    [UsersServiceFactoryKey as symbol]: {
      create: () => mockUsersService,
    },
  };

  beforeEach(() => {
    global.localStorage = localStorageMock;
    localStorageMock.getItem.mockReturnValue(
      JSON.stringify({
        id: 'test-id',
        expiresAtInSeconds: Date.now() / 1000 + 15 * 60 * 60,
        token: 'token',
      })
    );
  });

  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
    global.localStorage = originalStorage;
  });

  it('should display a heading', async () => {
    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const heading = getByRole('heading', { name: /onxgraph/i });

    expect(heading).toBeInTheDocument();
  });

  it('should display the navigation', async () => {
    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const navigation = getByRole('navigation');

    expect(navigation).toBeInTheDocument();
  });

  it('should display sidebar as expanded after rendering', async () => {
    const { getByRole, getByTestId } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const sidebar = getByTestId('sidebar');
    const collapseButton = getByRole('button', { name: /collapse/i });

    expect(sidebar).toBeInTheDocument();
    expect(sidebar.classList).not.toContain('collapsed');
    expect(collapseButton).toBeInTheDocument();
  });

  it('should display sidebar as collapsed after clicking collapse button', async () => {
    const { getByRole, getByTestId } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    await fireEvent.click(getByRole('button', { name: /collapse/i }));

    const sidebar = getByTestId('sidebar');
    const expandButton = getByRole('button', { name: /expand/i });

    expect(sidebar.classList).toContain('collapsed');
    expect(expandButton).toBeInTheDocument();
  });

  it('should display sidebar as expanded after clicking expand button', async () => {
    const { getByRole, getByTestId } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    await fireEvent.click(getByRole('button', { name: /collapse/i }));
    await fireEvent.click(getByRole('button', { name: /expand/i }));

    const sidebar = getByTestId('sidebar');
    const collapseButton = getByRole('button', { name: /collapse/i });

    expect(sidebar.classList).not.toContain('collapsed');
    expect(collapseButton).toBeInTheDocument();
  });

  it('should display user icon', async () => {
    const { getByTestId } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const userIcon = getByTestId('user-icon');

    expect(userIcon).toBeInTheDocument();
  });

  it('should display username when it is successfully retrieved', async () => {
    const { getByText } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      const username = getByText(/test-username/i);
      expect(username).toBeInTheDocument();
    });
  });

  it('should log user out if user not logged in when attempting to retrieve username', async () => {
    localStorageMock.getItem.mockReturnValue(null);

    await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const { push: pushMock } = useRouter();
    const userStore = useUserStore();

    expect(userStore.logUserOut).toHaveBeenCalled();
    expect(pushMock).toHaveBeenCalledWith({ name: 'login' });
  });

  it('should display user id when username is unavailable', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const mockUsersService = {
      getUser: () => ({ err: true, val: [new Error('Unable to get username')] }),
    };

    const { getByText } = await customRender(NavigationSidebar, {
      global: {
        provide: {
          [UsersServiceFactoryKey as symbol]: {
            create: () => mockUsersService,
          },
        },
      },
    });

    await waitFor(() => {
      const userId = getByText(/test-id/i);
      expect(userId).toBeInTheDocument();
    });
  });

  it('should display logout button', async () => {
    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const logoutButton = getByRole('button', { name: /logout/i });

    expect(logoutButton).toBeInTheDocument();
  });

  it('should log user out when logout button is clicked', async () => {
    const mockAuthService = {
      logout: vi.fn(),
    };

    mockAuthService.logout.mockResolvedValue({ err: false });

    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: {
          ...defaultProvide,
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const logoutButton = getByRole('button', { name: /logout/i });

    await fireEvent.click(logoutButton);

    const userStore = useUserStore();
    const { push: pushMock } = useRouter();

    await waitFor(() => {
      expect(userStore.logUserOut).toHaveBeenCalled();
      expect(pushMock).toHaveBeenCalledWith({ name: 'login' });
    });
  });

  it('should display alert if logout fails', async () => {
    const alertSpy = vi.spyOn(window, 'alert').mockImplementation(() => {});

    const mockAuthService = {
      logout: vi.fn(),
    };

    mockAuthService.logout.mockResolvedValue({ err: true, val: [new Error('Unable to log out')] });

    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: {
          ...defaultProvide,
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const logoutButton = getByRole('button', { name: /logout/i });

    await fireEvent.click(logoutButton);

    await waitFor(() => {
      expect(alertSpy).toHaveBeenCalled();
    });
  });

  it('should store expanded or collapsed state in user store', async () => {
    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const collapseButton = getByRole('button', { name: /collapse/i });
    const userStore = useUserStore();

    await fireEvent.click(collapseButton);

    expect(userStore.updateSidebarState).toHaveBeenCalledWith(false);

    const expandButton = getByRole('button', { name: /expand/i });
    await fireEvent.click(expandButton);

    expect(userStore.updateSidebarState).toHaveBeenCalledWith(true);
  });

  it('should display with expanded or collapsed state based on user store', async () => {
    const user = {
      id: 'test-id',
      expiresAtInSeconds: Date.now() / 1000 + 15 * 60 * 60,
      token: 'token',
      expanded: false,
    };

    localStorageMock.getItem.mockReturnValue(JSON.stringify(user));

    const { getByRole } = await customRender(NavigationSidebar, {
      global: {
        provide: defaultProvide,
      },
    });

    const expandButton = getByRole('button', { name: /expand/i });

    expect(expandButton).toBeInTheDocument();
  });
});

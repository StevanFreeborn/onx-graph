import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useUsersService } from '../../src/composables/useUsersService';
import { ClientFactoryKey } from '../../src/services/client';
import { UsersServiceFactoryKey } from '../../src/services/usersService';

describe('useUsersService', () => {
  const mockClientFactory = {
    create: vi.fn(),
  };

  const mockUsersServiceFactory = {
    create: vi.fn(),
  };

  const mockUserStore = {
    user: {
      token: 'your-test-token',
    },
    refreshAccessToken: vi.fn(),
  };

  const mountOptions = {
    global: {
      provide: {
        [ClientFactoryKey as symbol]: mockClientFactory,
        [UsersServiceFactoryKey as symbol]: mockUsersServiceFactory,
      },
    },
  };

  beforeEach(() => {
    mockUsersServiceFactory.create.mockImplementation(() => 'mock-users-service');
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should create usersService with proper configuration when store is provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { usersService: useUsersService(mockUserStore) };
        },
      },
      mountOptions
    );

    expect(mockClientFactory.create).toHaveBeenCalledWith(
      expect.objectContaining({
        authHeader: {
          Authorization: `Bearer ${mockUserStore.user.token}`,
        },
      })
    );
    expect(mockUsersServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.usersService).toBe('mock-users-service');
  });

  it('should create usersService with default configuration when store is not provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { usersService: useUsersService() };
        },
      },
      mountOptions
    );

    expect(mockClientFactory.create).toHaveBeenCalled();
    expect(mockUsersServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.usersService).toBe('mock-users-service');
  });

  it('should throw an error when dependencies are not injected', () => {
    mount(
      {
        template: '<div></div>',
        setup() {
          expect(() => useUsersService()).toThrowError('Failed to inject dependencies');
        },
      },
      {
        global: {
          provide: {
            [ClientFactoryKey as symbol]: undefined,
            [UsersServiceFactoryKey as symbol]: undefined,
          },
        },
      }
    );
  });
});

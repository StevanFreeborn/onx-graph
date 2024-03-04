import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useAuthService } from '../../src/composables/useAuthService';
import { AuthServiceFactoryKey } from './../../src/services/authService';
import { ClientFactoryKey } from './../../src/services/client';

describe('useAuthService', () => {
  const mockClientFactory = {
    create: vi.fn(),
  };

  const mockAuthServiceFactory = {
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
        [AuthServiceFactoryKey as symbol]: mockAuthServiceFactory,
      },
    },
  };

  beforeEach(() => {
    mockAuthServiceFactory.create.mockImplementation(() => 'mock-auth-service');
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should create authService with proper configuration when store is provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { authService: useAuthService(mockUserStore) };
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
    expect(mockAuthServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.authService).toBe('mock-auth-service');
  });

  it('should create authService with default configuration when store is not provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { authService: useAuthService() };
        },
      },
      mountOptions
    );

    expect(mockClientFactory.create).toHaveBeenCalled();
    expect(mockAuthServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.authService).toBe('mock-auth-service');
  });

  it('should throw an error when dependencies are not injected', () => {
    mount(
      {
        template: '<div></div>',
        setup() {
          expect(() => useAuthService()).toThrowError('Failed to inject dependencies');
        },
      },
      {
        global: {
          provide: {
            [ClientFactoryKey as symbol]: undefined,
            [AuthServiceFactoryKey as symbol]: undefined,
          },
        },
      }
    );
  });
});

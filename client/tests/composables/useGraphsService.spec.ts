import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useGraphsService } from '../../src/composables/useGraphsService';
import { ClientFactoryKey } from '../../src/services/client';
import { GraphsServiceFactoryKey } from './../../src/services/graphsService';

describe('useGraphsService', () => {
  const mockClientFactory = {
    create: vi.fn(),
  };

  const mockGraphsServiceFactory = {
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
        [GraphsServiceFactoryKey as symbol]: mockGraphsServiceFactory,
      },
    },
  };

  beforeEach(() => {
    mockGraphsServiceFactory.create.mockImplementation(() => 'mock-graphs-service');
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should create graphsService with proper configuration when store is provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { authService: useGraphsService(mockUserStore) };
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
    expect(mockGraphsServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.authService).toBe('mock-graphs-service');
  });

  it('should create graphsService with default configuration when store is not provided', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { authService: useGraphsService() };
        },
      },
      mountOptions
    );

    expect(mockClientFactory.create).toHaveBeenCalled();
    expect(mockGraphsServiceFactory.create).toHaveBeenCalledWith(mockClientFactory.create());
    expect(wrapper.vm.authService).toBe('mock-graphs-service');
  });

  it('should throw an error when dependencies are not injected', () => {
    mount(
      {
        template: '<div></div>',
        setup() {
          expect(() => useGraphsService()).toThrowError('Failed to inject dependencies');
        },
      },
      {
        global: {
          provide: {
            [ClientFactoryKey as symbol]: undefined,
            [GraphsServiceFactoryKey as symbol]: undefined,
          },
        },
      }
    );
  });
});

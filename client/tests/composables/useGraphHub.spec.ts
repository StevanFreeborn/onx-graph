import { createTestingPinia } from '@pinia/testing';
import { mount } from '@vue/test-utils';
import { afterEach } from 'node:test';
import { describe, expect, it, vi } from 'vitest';
import { useGraphHub } from '../../src/composables/useGraphHub';
import { AuthServiceFactoryKey } from '../../src/services/authService';
import { ClientFactoryKey } from '../../src/services/client';

describe('useGraphHub', () => {
  const mockClientFactory = {
    create: vi.fn(),
  };

  const mockAuthServiceFactory = {
    create: vi.fn(),
  };

  const mountOptions = {
    global: {
      provide: {
        [ClientFactoryKey as symbol]: mockClientFactory,
        [AuthServiceFactoryKey as symbol]: mockAuthServiceFactory,
      },
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
        }),
      ],
    },
  };

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should return a signal connection', () => {
    const wrapper = mount(
      {
        template: '<div></div>',
        setup() {
          return { connection: useGraphHub() };
        },
      },
      mountOptions
    );

    expect(wrapper.vm.connection).toBeDefined();
    expect(wrapper.vm.connection).toHaveProperty('start');
    expect(wrapper.vm.connection).toHaveProperty('on');
    expect(wrapper.vm.connection).toHaveProperty('stop');
  });
});

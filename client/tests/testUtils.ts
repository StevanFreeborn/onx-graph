import { createTestingPinia } from '@pinia/testing';
import { RenderOptions, render } from '@testing-library/vue';
import VNetworkGraph from 'v-network-graph';
import { vi } from 'vitest';
import { Component } from 'vue';
import { createRouter, createWebHistory } from 'vue-router';
import { routes } from '../src/router';
import { AuthServiceFactoryKey } from './../src/services/authService';
import { ClientFactoryKey } from './../src/services/client';

/**
 * Custom render function that provides some app level dependencies
 * for rendering components under test. You can override the default
 * render options by passing your specific options.
 * @param component - The component to render
 * @param options - The render options to add or overwrite the default options
 * @returns The render result
 */
export async function customRender(component: Component, options: RenderOptions = {}) {
  const router = createRouter({
    history: createWebHistory(),
    routes: routes,
  });

  mockMatchMediaOnWindow();

  const defaultProvides = {
    [AuthServiceFactoryKey as symbol]: {
      create: vi.fn(),
    },
    [ClientFactoryKey as symbol]: {
      create: vi.fn(),
    },
  };

  const defaultPlugins = [
    router,
    createTestingPinia({
      createSpy: vi.fn,
    }),
    VNetworkGraph,
  ];

  const { global, ...rest } = options;
  const { plugins, provide, ...restGlobal } = global;

  const renderResult = render(component, {
    global: {
      provide: {
        ...defaultProvides,
        ...provide,
      },
      plugins: [...defaultPlugins, ...plugins],
      ...restGlobal,
    },
    ...rest,
  });

  return renderResult;
}

// Mock matchMedia on window to avoid errors in tests
// that use window.matchMedia.
function mockMatchMediaOnWindow() {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  });
}

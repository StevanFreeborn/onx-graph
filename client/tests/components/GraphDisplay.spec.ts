import '@testing-library/jest-dom/vitest';
import { cleanup, waitFor } from '@testing-library/vue';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import GraphDisplay from '../../src/components/GraphDisplay.vue';
import { GraphsServiceFactoryKey } from '../../src/services/graphsService';
import { GraphStatus } from '../../src/types';
import { customRender } from '../testUtils';

describe('GraphDisplay', () => {
  const localStorageMock = {
    getItem: vi.fn(),
    setItem: vi.fn(),
    clear: vi.fn(),
    removeItem: vi.fn(),
    key: vi.fn(),
    length: 0,
  };

  const originalStorage = global.localStorage;

  const mockGraph = {
    id: 'test-id',
    name: 'Test Graph',
    createdAt: '2021-01-01T00:00:00Z',
    updatedAt: '2021-01-01T00:00:00Z',
    status: 0,
  };

  const mockGraphsService = {
    getGraph: vi.fn(),
  };

  const defaultProvide = {
    [GraphsServiceFactoryKey as symbol]: {
      create: () => mockGraphsService,
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

  it('should display a loading spinner when loading', async () => {
    mockGraphsService.getGraph.mockImplementationOnce(async () => {
      await new Promise(() => setTimeout(() => {}, 1000));
      return { err: false, val: mockGraph };
    });

    const { getByTitle, getByText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    const spinner = getByTitle(/spinning loader/i);
    const message = getByText(/loading/i);

    expect(spinner).toBeInTheDocument();
    expect(message).toBeInTheDocument();
  });

  it('should display error message when graph fails to load', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: true,
      val: [new Error('Failed to get graph.')],
    });

    const { getByText, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByText(/error loading the graph/i)).toBeInTheDocument();
      expect(getByRole('button', { name: /try again/i })).toBeInTheDocument();
      expect(console.error).toHaveBeenCalled();
    });
  });

  it('should display the graph when loaded and graph has status of built', async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.Built },
    });

    const { getByTestId } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
        stubs: {
          OnxGraph: {
            template: `<div>graph</div>`,
          },
          GraphActionsMenu: {
            template: `<div>actions menu</div>`,
          },
        },
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-${mockGraph.id}`)).toBeInTheDocument();
    });
  });

  it('should display the graph monitor when loaded and graph has status of building', async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.Building },
    });

    const { getByText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
        stubs: {
          GraphMonitor: {
            template: `<div>graph monitor</div>`,
          },
        },
      },
    });

    await waitFor(() => {
      expect(getByText(/graph monitor/i)).toBeInTheDocument();
    });
  });

  it('should display not built graph message and build graph button when loaded and graph has status of not built', async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.NotBuilt },
    });

    const { getByText, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
        stubs: {
          GraphActionsMenu: {
            template: `<div>actions menu</div>`,
          },
        },
      },
    });

    await waitFor(() => {
      expect(getByText(/graph has not been built/i)).toBeInTheDocument();
      expect(getByRole('button', { name: /build graph/i })).toBeInTheDocument();
    });
  });
});

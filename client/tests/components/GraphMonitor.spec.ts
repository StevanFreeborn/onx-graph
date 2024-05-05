import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import GraphMonitor from '../../src/components/GraphMonitor.vue';
import { customRender } from '../testUtils';

const connectionMock = {
  start: vi.fn(),
  on: vi.fn(),
  invoke: vi.fn(),
  stop: vi.fn(),
};

vi.mock('../../src/composables/useGraphHub', async importOriginal => {
  const actual = await importOriginal<typeof import('../../src/composables/useGraphHub')>();

  return {
    ...actual,
    useGraphHub: () => connectionMock,
  };
});

describe('GraphMonitor', () => {
  afterEach(() => {
    cleanup();
  });

  const graphMonitorStub = {
    template: '<div>graph monitor loader</div>',
  };

  it('should start the connection on mount', async () => {
    await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    expect(connectionMock.start).toHaveBeenCalled();
  });

  it('should log error if connection fails to start', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    connectionMock.start.mockImplementationOnce(() => {
      throw new Error('test error');
    });

    await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    expect(console.error).toHaveBeenCalled();
  });

  it('should join the graph group on mount', async () => {
    const graphId = 'test-id';

    await customRender(GraphMonitor, {
      props: {
        graphId: graphId,
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    expect(connectionMock.invoke).toHaveBeenCalledWith('JoinGraph', graphId);
  });

  it('should stop the connection on unmount', async () => {
    const { unmount } = await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    unmount();

    expect(connectionMock.stop).toHaveBeenCalled();
  });

  it('should log error if connection fails to stop', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    connectionMock.stop.mockImplementationOnce(() => {
      throw new Error('test error');
    });

    const { unmount } = await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    unmount();

    expect(console.error).toHaveBeenCalled();
  });

  it('should register the connection event handlers', async () => {
    await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    expect(connectionMock.on).toHaveBeenCalledWith('ReceiveUpdate', expect.any(Function));
    expect(connectionMock.on).toHaveBeenCalledWith('GraphBuilt', expect.any(Function));
    expect(connectionMock.on).toHaveBeenCalledWith('GraphError', expect.any(Function));
  });

  it('should render the loader while the connection is starting', async () => {
    const { getByText } = await customRender(GraphMonitor, {
      props: {
        graphId: 'test-id',
      },
      global: {
        stubs: {
          GraphMonitorLoader: graphMonitorStub,
        },
      },
    });

    expect(getByText('graph monitor loader')).toBeInTheDocument();
  });
});

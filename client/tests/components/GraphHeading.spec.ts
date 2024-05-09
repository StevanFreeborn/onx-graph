import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import GraphHeading from '../../src/components/GraphHeading.vue';
import { GraphsServiceFactoryKey } from '../../src/services/graphsService';
import { GraphStatus } from '../../src/types';
import { customRender } from '../testUtils';

describe('GraphHeading', () => {
  const mockGraphService = {
    getGraphKey: vi.fn(),
  };

  const mockGraphServiceFactory = {
    create: () => mockGraphService,
  };

  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display the graph name', async () => {
    const { getByText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const name = getByText(/test graph/i);

    expect(name).toBeInTheDocument();
  });

  it('should display graph status when built', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const status = getByTitle(/built/i);

    expect(status).toBeInTheDocument();
  });

  it('should display graph status when building', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Building,
      },
    });

    const status = getByTitle(/building/i);

    expect(status).toBeInTheDocument();
  });

  it('should display graph status when not built', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.NotBuilt,
      },
    });

    const status = getByTitle(/not built/i);

    expect(status).toBeInTheDocument();
  });

  it("should display a placeholder for the graph's api key", async () => {
    const { getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const apiKey = getByLabelText(/api key/i);

    expect(apiKey).toBeInTheDocument();
  });

  it('should display a show API key button', async () => {
    const { getByText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const showApiKeyButton = getByText(/show api key/i);

    expect(showApiKeyButton).toBeInTheDocument();
  });

  it('should display a hide API key button after clicking show API key button', async () => {
    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: 'test' },
    });

    const { getByText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: mockGraphServiceFactory,
        },
      },
    });

    const showApiKeyButton = getByText(/show api key/i);

    await fireEvent.click(showApiKeyButton);

    const hideApiKeyButton = getByText(/hide api key/i);

    expect(hideApiKeyButton).toBeInTheDocument();
  });

  it('should retrieve and display the graph API key after clicking show API key button', async () => {
    const testApiKey = 'test-api-key';

    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: testApiKey },
    });

    const { getByText, getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: mockGraphServiceFactory,
        },
      },
    });

    const showApiKeyButton = getByText(/show api key/i);

    await fireEvent.click(showApiKeyButton);

    await waitFor(() => {
      const apiKey = getByLabelText(/api key/i);

      expect(apiKey).toHaveValue('test-api-key');
      expect(mockGraphService.getGraphKey).toHaveBeenCalledTimes(1);
    });
  });

  it('should display an error message in place of API key value when failed to retrieve API key', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: true,
      val: [new Error('Failed to get graph key.')],
    });

    const { getByText, getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: mockGraphServiceFactory,
        },
      },
    });

    const showApiKeyButton = getByText(/show api key/i);

    await fireEvent.click(showApiKeyButton);

    await waitFor(() => {
      const apiKey = getByLabelText(/api key/i);

      expect(apiKey).toHaveValue('Hmmm...Something went wrong.');
      expect(mockGraphService.getGraphKey).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalled();
    });
  });
});

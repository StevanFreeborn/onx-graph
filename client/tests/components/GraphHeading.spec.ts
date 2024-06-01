import '@testing-library/jest-dom/vitest';
import userEvent from '@testing-library/user-event';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import GraphHeading from '../../src/components/GraphHeading.vue';
import { GraphsServiceFactoryKey } from '../../src/services/graphsService';
import { GraphStatus } from '../../src/types';
import { customRender } from '../testUtils';

describe('GraphHeading', () => {
  const mockGraphService = {
    getGraphKey: vi.fn(),
    updateGraphKey: vi.fn(),
  };

  const mockGraphServiceFactory = {
    create: () => mockGraphService,
  };

  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display the graph name', async () => {
    const { getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const nameInput = getByLabelText(/graph name/i);

    expect(nameInput).toBeInTheDocument();
    expect(nameInput).toHaveValue('Test Graph');
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

  it('should replace real api key with fake api key when hiding API key', async () => {
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

    const hideApiKeyButton = getByText(/hide api key/i);

    await fireEvent.click(hideApiKeyButton);

    const apiKey = getByLabelText(/api key/i);
    expect(apiKey).toHaveValue('********************************');
  });

  it("should display the graph's name in a readonly input field", async () => {
    const { getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const nameInput = getByLabelText(/graph name/i);

    expect(nameInput).toHaveAttribute('readonly');
  });

  it("should make the graph's name editable when focusing on the input field", async () => {
    const { getByLabelText } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const nameInput = getByLabelText(/graph name/i);

    await fireEvent.focus(nameInput);

    expect(nameInput).not.toHaveAttribute('readonly');
  });

  it('should update the graph name when the input field is blurred', async () => {
    const { getByLabelText, emitted } = await customRender(GraphHeading, {
      props: {
        id: '1',
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const nameInput = getByLabelText(/graph name/i);

    await fireEvent.focus(nameInput);

    await fireEvent.blur(nameInput);

    expect(emitted().updateName).toHaveLength(1);
  });

  it("should not update the graph's api key when empty text is entered into the input field", async () => {
    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: 'test' },
    });

    const { getByLabelText, getByRole } = await customRender(GraphHeading, {
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

    const showApiKeyButton = getByRole('button', { name: /show api key/i });

    await fireEvent.click(showApiKeyButton);

    const apiKeyInput = getByLabelText(/api key/i);

    await userEvent.click(apiKeyInput);
    await userEvent.clear(apiKeyInput);

    expect(apiKeyInput).toHaveValue('');
    expect(mockGraphService.updateGraphKey).not.toHaveBeenCalled();
  });

  it("should update the graph's api key when text is entered into the input field", async () => {
    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: 'test' },
    });

    mockGraphService.updateGraphKey.mockResolvedValueOnce({ err: false });

    const { getByLabelText, getByRole } = await customRender(GraphHeading, {
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

    const showApiKeyButton = getByRole('button', { name: /show api key/i });

    await fireEvent.click(showApiKeyButton);

    const apiKeyInput = getByLabelText(/api key/i);

    await userEvent.click(apiKeyInput);
    await userEvent.clear(apiKeyInput);
    await userEvent.type(apiKeyInput, 'new-api-key');

    expect(apiKeyInput).toHaveValue('new-api-key');

    await waitFor(() => {
      expect(mockGraphService.updateGraphKey).toHaveBeenCalledWith('1', 'new-api-key');
    });
  });

  it("should update the graph's api key when text is entered but should throttle requests", async () => {
    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: 'test' },
    });

    mockGraphService.updateGraphKey.mockResolvedValueOnce({ err: false });
    mockGraphService.updateGraphKey.mockResolvedValueOnce({ err: false });

    const { getByLabelText, getByRole } = await customRender(GraphHeading, {
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

    const showApiKeyButton = getByRole('button', { name: /show api key/i });

    await fireEvent.click(showApiKeyButton);

    const apiKeyInput = getByLabelText(/api key/i);

    await userEvent.click(apiKeyInput);
    await userEvent.clear(apiKeyInput);

    for (const letter of 'new-api-key') {
      await userEvent.type(apiKeyInput, letter);
    }

    expect(apiKeyInput).toHaveValue('new-api-key');

    await waitFor(() => {
      expect(mockGraphService.updateGraphKey).toHaveBeenCalledTimes(1);
      expect(mockGraphService.updateGraphKey).toHaveBeenNthCalledWith(1, '1', 'new-api-key');
    });
  });

  it('should log an error when failed to update the graph key', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    mockGraphService.getGraphKey.mockResolvedValueOnce({
      err: false,
      val: { key: 'test' },
    });

    mockGraphService.updateGraphKey.mockResolvedValueOnce({
      err: true,
      val: [new Error('Failed to update graph key.')],
    });

    const { getByLabelText, getByRole } = await customRender(GraphHeading, {
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

    const showApiKeyButton = getByRole('button', { name: /show api key/i });

    await fireEvent.click(showApiKeyButton);

    const apiKeyInput = getByLabelText(/api key/i);

    await userEvent.click(apiKeyInput);
    await userEvent.clear(apiKeyInput);
    await userEvent.type(apiKeyInput, 'new-api-key');

    await waitFor(() => {
      expect(console.error).toHaveBeenCalled();
    });
  });
});

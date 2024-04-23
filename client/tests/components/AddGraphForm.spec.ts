import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import AddGraphForm from '../../src/components/AddGraphForm.vue';
import { GraphsServiceFactoryKey } from '../../src/services/graphsService';
import { customRender } from '../testUtils';

vi.mock('vue-router', async importOriginal => {
  const actual = await importOriginal<typeof import('vue-router')>();
  const mock = vi.fn();
  return {
    ...actual,
    useRouter: () => ({ push: mock }),
  };
});

describe('AddGraphForm', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display an add graph form', async () => {
    const { getByRole } = await customRender(AddGraphForm);
    const addGraphForm = getByRole('form', { name: 'add graph form' });
    expect(addGraphForm).toBeInTheDocument();
  });

  it('should display a name input', async () => {
    const { getByRole } = await customRender(AddGraphForm);
    const nameInput = getByRole('textbox', { name: /name/i });
    expect(nameInput).toBeInTheDocument();
  });

  describe('name input', () => {
    it('should be required', async () => {
      const { getByRole } = await customRender(AddGraphForm);
      const nameInput = getByRole('textbox', { name: /name/i });
      expect(nameInput).toBeRequired();
    });

    it('should have a type of text', async () => {
      const { getByRole } = await customRender(AddGraphForm);
      const nameInput = getByRole('textbox', { name: /name/i });
      expect(nameInput).toHaveAttribute('type', 'text');
    });
  });

  it('should display an api key input', async () => {
    const { getByLabelText } = await customRender(AddGraphForm);
    const apiKeyInput = getByLabelText(/api key/i);
    expect(apiKeyInput).toBeInTheDocument();
  });

  describe('api key input', () => {
    it('should be required', async () => {
      const { getByLabelText } = await customRender(AddGraphForm);
      const apiKeyInput = getByLabelText(/api key/i);
      expect(apiKeyInput).toBeRequired();
    });

    it('should have a type of password', async () => {
      const { getByLabelText } = await customRender(AddGraphForm);
      const apiKeyInput = getByLabelText(/api key/i);
      expect(apiKeyInput).toHaveAttribute('type', 'password');
    });
  });

  it('should display an add button', async () => {
    const { getByRole } = await customRender(AddGraphForm);
    const addButton = getByRole('button', { name: /add/i });
    expect(addButton).toBeInTheDocument();
  });

  it('should display error message if form is submitted with no name', async () => {
    const { getByRole, getByText } = await customRender(AddGraphForm);
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.click(addButton);

    const errorMessage = getByText(/name is required/i);
    expect(errorMessage).toBeInTheDocument();
  });

  it('should display error message if form is submitted with no api key', async () => {
    const { getByRole, getByText } = await customRender(AddGraphForm);
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.click(addButton);

    const errorMessage = getByText(/api key is required/i);
    expect(errorMessage).toBeInTheDocument();
  });

  it('should hide error message when name is entered', async () => {
    const { getByRole, queryByText, getByText } = await customRender(AddGraphForm);
    const nameInput = getByRole('textbox', { name: /name/i });
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.click(addButton);

    expect(getByText(/name is required/i)).toBeInTheDocument();

    await fireEvent.update(nameInput, 'test');

    expect(queryByText(/name is required/i)).not.toBeInTheDocument();
  });

  it('should hide error message when api key is entered', async () => {
    const { getByRole, queryByText, getByText, getByLabelText } = await customRender(AddGraphForm);
    const apiKeyInput = getByLabelText(/api key/i);
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.click(addButton);

    expect(getByText(/api key is required/i)).toBeInTheDocument();

    await fireEvent.update(apiKeyInput, 'test');

    expect(queryByText(/api key is required/i)).not.toBeInTheDocument();
  });

  it('should disable the add button when the form is submitting', async () => {
    const mockGraphsService = {
      addGraph: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: false, val: ['Error occurred'] });
      }, 100)
    );

    mockGraphsService.addGraph.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByLabelText } = await customRender(AddGraphForm, {
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: {
            create: () => mockGraphsService,
          },
        },
      },
    });

    const testName = 'test name';
    const testApiKey = 'test api key';

    const nameInput = getByRole('textbox', { name: /name/i });
    const apiKeyInput = getByLabelText(/api key/i);
    const addButton = getByRole('button', { name: /add/i });

    expect(addButton).toBeEnabled();

    await fireEvent.update(nameInput, testName);
    await fireEvent.update(apiKeyInput, testApiKey);
    await fireEvent.click(addButton);

    expect(addButton).toBeDisabled();

    expect(mockGraphsService.addGraph).toHaveBeenCalledTimes(1);
    expect(mockGraphsService.addGraph).toHaveBeenCalledWith(testName, testApiKey);

    await waitFor(() => {
      expect(addButton).toBeEnabled();
    });
  });

  it('should display an error message if the graph service returns an error', async () => {
    const mockGraphsService = {
      addGraph: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: true, val: [new Error('Error occurred')] });
      }, 100)
    );

    mockGraphsService.addGraph.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByLabelText, getByText } = await customRender(AddGraphForm, {
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: {
            create: () => mockGraphsService,
          },
        },
      },
    });

    const testName = 'test name';
    const testApiKey = 'test api key';

    const nameInput = getByRole('textbox', { name: /name/i });
    const apiKeyInput = getByLabelText(/api key/i);
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.update(nameInput, testName);
    await fireEvent.update(apiKeyInput, testApiKey);
    await fireEvent.click(addButton);

    expect(mockGraphsService.addGraph).toHaveBeenCalledTimes(1);
    expect(mockGraphsService.addGraph).toHaveBeenCalledWith(testName, testApiKey);

    await waitFor(() => {
      expect(getByText(/error occurred/i)).toBeInTheDocument();
    });
  });

  it('should route user to graph page if graph is added successfully', async () => {
    const mockGraphsService = {
      addGraph: vi.fn(),
    };

    const successResponse = {
      err: false,
      val: { id: 'test-id' },
    };

    mockGraphsService.addGraph.mockReturnValueOnce(successResponse);

    const { getByRole, getByLabelText } = await customRender(AddGraphForm, {
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: {
            create: () => mockGraphsService,
          },
        },
      },
    });

    const testName = 'test name';
    const testApiKey = 'test api key';

    const nameInput = getByRole('textbox', { name: /name/i });
    const apiKeyInput = getByLabelText(/api key/i);
    const addButton = getByRole('button', { name: /add/i });

    await fireEvent.update(nameInput, testName);
    await fireEvent.update(apiKeyInput, testApiKey);
    await fireEvent.click(addButton);

    expect(mockGraphsService.addGraph).toHaveBeenCalledTimes(1);
    expect(mockGraphsService.addGraph).toHaveBeenCalledWith(testName, testApiKey);

    const { push: pushMock } = useRouter();

    expect(pushMock).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith('/graphs/test-id');
  });
});

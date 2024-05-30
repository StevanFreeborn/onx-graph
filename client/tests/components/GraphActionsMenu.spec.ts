import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import GraphActionsMenu from '../../src/components/GraphActionsMenu.vue';
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

describe('GraphActionsMenu', () => {
  const mockGraphService = {
    deleteGraph: vi.fn(),
  };

  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display a button to open and close menu', async () => {
    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    expect(menuButton).toBeInTheDocument();
  });

  it('should not display a menu when initially rendered', async () => {
    const { queryByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menu = queryByRole('menu');

    expect(menu).not.toBeInTheDocument();
  });

  it('should display a menu when the button is clicked', async () => {
    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const menu = getByRole('menu');

    expect(menu).toBeInTheDocument();
  });

  it('should display a menu with a delete graph button', async () => {
    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteButton = getByRole('button', { name: 'Delete Graph' });

    expect(deleteButton).toBeInTheDocument();
  });

  it('should hide menu when delete graph button is clicked', async () => {
    const { getByRole, queryByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteButton = getByRole('button', { name: 'Delete Graph' });

    await fireEvent.click(deleteButton);

    const menu = queryByRole('menu');

    expect(menu).not.toBeInTheDocument();
  });

  it('should display a confirmation dialog when delete graph button is clicked', async () => {
    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteButton = getByRole('button', { name: 'Delete Graph' });

    await fireEvent.click(deleteButton);

    const dialog = getByRole('alertdialog');

    expect(dialog).toBeInTheDocument();
  });

  it('should delete graph and router to graphs when deletion is confirmed', async () => {
    mockGraphService.deleteGraph.mockResolvedValue({ err: false });

    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: {
            create: () => mockGraphService,
          },
        },
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteGraphButton = getByRole('button', { name: 'Delete Graph' });

    await fireEvent.click(deleteGraphButton);

    const deleteConfirmButton = getByRole('button', { name: 'Delete' });

    await fireEvent.click(deleteConfirmButton);

    const { push: pushMock } = useRouter();

    await waitFor(() => {
      expect(mockGraphService.deleteGraph).toHaveBeenCalledTimes(1);
      expect(mockGraphService.deleteGraph).toHaveBeenCalledWith('test-graph');
      expect(pushMock).toHaveBeenCalledTimes(1);
      expect(pushMock).toHaveBeenCalledWith('/graphs');
    });
  });

  it('should display alert and log error when deletion fails', async () => {
    mockGraphService.deleteGraph.mockResolvedValue({
      err: true,
      val: [new Error('Failed to delete graph.')],
    });

    vi.spyOn(window, 'alert').mockImplementation(() => {});

    vi.spyOn(console, 'error').mockImplementation(() => {});

    const { getByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
      global: {
        provide: {
          [GraphsServiceFactoryKey as symbol]: {
            create: () => mockGraphService,
          },
        },
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteGraphButton = getByRole('button', { name: 'Delete Graph' });

    await fireEvent.click(deleteGraphButton);

    const deleteConfirmButton = getByRole('button', { name: 'Delete' });

    await fireEvent.click(deleteConfirmButton);

    await waitFor(() => {
      expect(mockGraphService.deleteGraph).toHaveBeenCalledTimes(1);
      expect(mockGraphService.deleteGraph).toHaveBeenCalledWith('test-graph');
      expect(window.alert).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalledTimes(1);
      expect(console.error).toHaveBeenCalledWith(new Error('Failed to delete graph.'));
    });
  });

  it('should hide confirmation dialog when deletion is cancelled', async () => {
    const { getByRole, queryByRole } = await customRender(GraphActionsMenu, {
      props: {
        graphId: 'test-graph',
      },
    });

    const menuButton = getByRole('button', { name: 'Toggle Actions Menu' });

    await fireEvent.click(menuButton);

    const deleteButton = getByRole('button', { name: 'Delete Graph' });

    await fireEvent.click(deleteButton);

    const cancelDeleteButton = getByRole('button', { name: 'Cancel' });

    await fireEvent.click(cancelDeleteButton);

    const dialog = queryByRole('alertdialog');

    expect(dialog).not.toBeInTheDocument();
  });
});

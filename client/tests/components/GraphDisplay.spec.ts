import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor, within } from '@testing-library/vue';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import GraphDisplay from '../../src/components/GraphDisplay.vue';
import { GraphNotFoundError, GraphsServiceFactoryKey } from '../../src/services/graphsService';
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
    nodes: [],
    edgesMap: {},
    layout: {},
  };

  const mockGraphsService = {
    getGraph: vi.fn(),
    updateGraph: vi.fn(),
    refreshGraph: vi.fn(),
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
          GraphFilter: {
            template: `<div>filter</div>`,
          },
        },
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${mockGraph.id}`)).toBeInTheDocument();
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

  it('should attempt to retrieve graph again when initial load fails and try again button is clicked', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});

    mockGraphsService.getGraph.mockReturnValue({
      err: true,
      val: [new Error('Failed to get graph.')],
    });

    const { getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(async () => {
      const tryAgainButton = getByRole('button', { name: /try again/i });
      await fireEvent.click(tryAgainButton);
    });

    await waitFor(() => {
      expect(mockGraphsService.getGraph).toHaveBeenCalledTimes(2);
      expect(console.error).toHaveBeenCalled();
    });
  });

  it('should display not found message when graph is not found', async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: true,
      val: [new GraphNotFoundError()],
    });

    const { getByText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByText(/hmm...doesn't look like that graph exists/i)).toBeInTheDocument();
    });
  });

  it('should retrieve graph when graph monitor emits graph processed', async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.Building },
    });

    const { getByText, getByTestId } = await customRender(GraphDisplay, {
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
      expect(mockGraphsService.getGraph).toHaveBeenCalledTimes(1);
    });

    await waitFor(() => {
      expect(getByText(/graph monitor/i)).toBeInTheDocument();
    });

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.Built },
    });

    const graphMonitor = getByText('graph monitor');
    await fireEvent(graphMonitor, new CustomEvent('graph-processed'));

    await waitFor(() => {
      const graph = getByTestId(`graph-display-${mockGraph.id}`);

      expect(graph).toBeInTheDocument();
      expect(mockGraphsService.getGraph).toHaveBeenCalledTimes(2);
    });
  });

  it("should not update graph's name when graph heading emits update:name event and graph name is unchanged", async () => {
    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: { ...mockGraph, status: GraphStatus.Built },
    });

    const { getByLabelText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByLabelText(/graph name/i)).toBeInTheDocument();
    });

    const graphNameInput = getByLabelText(/graph name/i);

    await fireEvent.update(graphNameInput, mockGraph.name);
    await fireEvent.blur(graphNameInput);

    expect(mockGraphsService.updateGraph).not.toHaveBeenCalled();
  });

  it("should update graph's name when graph heading emits update:name event and graph name is changed", async () => {
    const testGraph = { ...mockGraph, status: GraphStatus.Built };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByLabelText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByLabelText(/graph name/i)).toBeInTheDocument();
    });

    const newName = 'New Graph Name';
    const updatedGraph = { ...testGraph, name: newName };

    mockGraphsService.updateGraph.mockReturnValueOnce({
      err: false,
      val: updatedGraph,
    });

    const graphNameInput = getByLabelText(/graph name/i);

    await fireEvent.update(graphNameInput, newName);
    await fireEvent.blur(graphNameInput);

    expect(mockGraphsService.updateGraph).toHaveBeenCalledWith(updatedGraph);
    expect(graphNameInput).toHaveValue(newName);
  });

  it('should display alert and log error when graph name update fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.spyOn(window, 'alert').mockImplementation(() => {});

    const testGraph = { ...mockGraph, status: GraphStatus.Built };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByLabelText } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByLabelText(/graph name/i)).toBeInTheDocument();
    });

    const newName = 'New Graph Name';
    const updatedGraph = { ...testGraph, name: newName };

    mockGraphsService.updateGraph.mockReturnValueOnce({
      err: true,
      val: [new Error('Failed to update graph name.')],
    });

    const graphNameInput = getByLabelText(/graph name/i);
    await fireEvent.update(graphNameInput, newName);
    await fireEvent.blur(graphNameInput);

    await waitFor(() => {
      expect(mockGraphsService.updateGraph).toHaveBeenCalledWith(updatedGraph);
      expect(console.error).toHaveBeenCalled();
      expect(window.alert).toHaveBeenCalledWith('Failed to update graph name.\n');
    });
  });

  it('should rebuild graph when graph is refreshed', async () => {
    const testGraph = { ...mockGraph, status: GraphStatus.Built };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByRole, getByText, getByTestId } = await customRender(GraphDisplay, {
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
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    mockGraphsService.refreshGraph.mockReturnValueOnce({
      err: false,
      val: true,
    });

    const actionsMenu = getByRole('button', { name: /toggle actions menu/i });
    await fireEvent.click(actionsMenu);

    const refreshButton = getByRole('button', { name: /refresh graph/i });
    await fireEvent.click(refreshButton);

    await waitFor(() => {
      expect(getByText(/graph monitor/i)).toBeInTheDocument();
      expect(mockGraphsService.refreshGraph).toHaveBeenCalledWith(testGraph.id);
    });
  });

  it('should display alert and log error when graph refresh fails', async () => {
    vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.spyOn(window, 'alert').mockImplementation(() => {});

    const testGraph = { ...mockGraph, status: GraphStatus.Built };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByRole, getByTestId, queryByText } = await customRender(GraphDisplay, {
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
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    mockGraphsService.refreshGraph.mockReturnValueOnce({
      err: true,
      val: [new Error('Failed to refresh graph.')],
    });

    const actionsMenu = getByRole('button', { name: /toggle actions menu/i });
    await fireEvent.click(actionsMenu);

    const refreshButton = getByRole('button', { name: /refresh graph/i });
    await fireEvent.click(refreshButton);

    await waitFor(() => {
      expect(mockGraphsService.refreshGraph).toHaveBeenCalledWith(testGraph.id);
      expect(console.error).toHaveBeenCalled();
      expect(window.alert).toHaveBeenCalledWith('Failed to refresh graph.\n');
      expect(queryByText(/graph monitor/i)).not.toBeInTheDocument();
    });
  });

  it('should remove node from graph when node is filtered', async () => {
    const testGraph = {
      ...mockGraph,
      status: GraphStatus.Built,
      nodes: [
        { href: null, id: 1, name: 'Roles' },
        { href: null, id: 2, name: 'Users' },
      ],
    };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByTestId, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    const graph = getByTestId(`graph-${testGraph.id}`);
    expect(graph).toBeInTheDocument();

    const nodes = graph.querySelectorAll('.v-ng-node');
    expect(nodes).toHaveLength(testGraph.nodes.length);

    const nodeLabels = graph.querySelectorAll('.v-ng-node-label');
    expect(nodeLabels).toHaveLength(testGraph.nodes.length);

    for (const label of nodeLabels) {
      expect(testGraph.nodes.some(node => label.textContent?.includes(node.name))).toBe(true);
    }

    const openFilterButton = getByRole('button', { name: /open filter/i });
    await fireEvent.click(openFilterButton);

    const usersCheckbox = getByRole('checkbox', { name: /users/i });
    await fireEvent.click(usersCheckbox);

    const filteredNodes = graph.querySelectorAll('.v-ng-node');
    expect(filteredNodes).toHaveLength(1);

    const filteredNodeLabels = graph.querySelectorAll('.v-ng-node-label');
    expect(filteredNodeLabels).toHaveLength(1);
    expect(filteredNodeLabels[0].textContent).toContain('Roles');
  });

  it('should add node back to graph when node is unfiltered', async () => {
    const testGraph = {
      ...mockGraph,
      status: GraphStatus.Built,
      nodes: [
        { href: null, id: 1, name: 'Roles' },
        { href: null, id: 2, name: 'Users' },
      ],
    };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByTestId, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    const graph = getByTestId(`graph-${testGraph.id}`);
    expect(graph).toBeInTheDocument();

    const nodes = graph.querySelectorAll('.v-ng-node');
    expect(nodes).toHaveLength(testGraph.nodes.length);

    const nodeLabels = graph.querySelectorAll('.v-ng-node-label');
    expect(nodeLabels).toHaveLength(testGraph.nodes.length);

    for (const label of nodeLabels) {
      expect(testGraph.nodes.some(node => label.textContent?.includes(node.name))).toBe(true);
    }

    const openFilterButton = getByRole('button', { name: /open filter/i });
    await fireEvent.click(openFilterButton);

    const usersCheckbox = getByRole('checkbox', { name: /users/i });
    await fireEvent.click(usersCheckbox);

    const filteredNodes = graph.querySelectorAll('.v-ng-node');
    expect(filteredNodes).toHaveLength(1);

    const filteredNodeLabels = graph.querySelectorAll('.v-ng-node-label');
    expect(filteredNodeLabels).toHaveLength(1);
    expect(filteredNodeLabels[0].textContent).toContain('Roles');

    await fireEvent.click(usersCheckbox);

    const unfilteredNodes = graph.querySelectorAll('.v-ng-node');
    expect(unfilteredNodes).toHaveLength(2);

    const unfilteredNodeLabels = graph.querySelectorAll('.v-ng-node-label');
    expect(unfilteredNodeLabels).toHaveLength(2);

    for (const label of unfilteredNodeLabels) {
      expect(testGraph.nodes.some(node => label.textContent?.includes(node.name))).toBe(true);
    }
  });

  it('should remove edge from graph when edge references filtered node', async () => {
    const testGraph = {
      ...mockGraph,
      status: GraphStatus.Built,
      nodes: [
        { href: null, id: 1, name: 'Roles' },
        { href: null, id: 2, name: 'Users' },
      ],
      edgesMap: {
        1: [
          {
            multiplicity: 0,
            referencedAppId: 2,
            id: 44,
            appId: 1,
            name: 'Created By',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
        2: [
          {
            multiplicity: 1,
            referencedAppId: 1,
            id: 30,
            appId: 2,
            name: 'Roles',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
      },
    };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByTestId, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    const graph = getByTestId(`graph-${testGraph.id}`);
    expect(graph).toBeInTheDocument();

    const edges = graph.querySelectorAll('.v-ng-edge');
    expect(edges).toHaveLength(2);

    const openFilterButton = getByRole('button', { name: /open filter/i });
    await fireEvent.click(openFilterButton);

    const usersCheckbox = getByRole('checkbox', { name: /users/i });
    await fireEvent.click(usersCheckbox);

    const filteredEdges = graph.querySelectorAll('.v-ng-edge');
    expect(filteredEdges).toHaveLength(0);
  });

  it('should remove edge from graph when edge is filtered', async () => {
    const testGraph = {
      ...mockGraph,
      status: GraphStatus.Built,
      nodes: [
        { href: null, id: 1, name: 'Roles' },
        { href: null, id: 2, name: 'Users' },
      ],
      edgesMap: {
        1: [
          {
            multiplicity: 0,
            referencedAppId: 2,
            id: 44,
            appId: 1,
            name: 'Created By',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
        2: [
          {
            multiplicity: 1,
            referencedAppId: 1,
            id: 30,
            appId: 2,
            name: 'Roles',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
      },
    };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByTestId, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    const graph = getByTestId(`graph-${testGraph.id}`);
    expect(graph).toBeInTheDocument();

    const edges = graph.querySelectorAll('.v-ng-edge');
    expect(edges).toHaveLength(2);

    const filter = getByTestId('graph-filter');
    expect(filter).toBeInTheDocument();

    const openFilterButton = getByRole('button', { name: /open filter/i });
    await fireEvent.click(openFilterButton);

    const listItems = within(filter).getAllByRole('listitem');
    const rolesListItem = listItems.find(item => item.textContent?.includes('Roles'));
    expect(rolesListItem).toBeInTheDocument();

    const expandRolesButton = within(rolesListItem!).getByRole('button', { name: /expand/i });
    await fireEvent.click(expandRolesButton);

    const createdByCheckbox = getByRole('checkbox', { name: /created by/i });
    await fireEvent.click(createdByCheckbox);

    const filteredEdges = graph.querySelectorAll('.v-ng-edge');
    expect(filteredEdges).toHaveLength(1);
  });

  it('should add edge back to graph when edge is unfiltered', async () => {
    const testGraph = {
      ...mockGraph,
      status: GraphStatus.Built,
      nodes: [
        { href: null, id: 1, name: 'Roles' },
        { href: null, id: 2, name: 'Users' },
      ],
      edgesMap: {
        1: [
          {
            multiplicity: 0,
            referencedAppId: 2,
            id: 44,
            appId: 1,
            name: 'Created By',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
        2: [
          {
            multiplicity: 1,
            referencedAppId: 1,
            id: 30,
            appId: 2,
            name: 'Roles',
            type: 500,
            status: 0,
            isRequired: false,
            isUnique: false,
          },
        ],
      },
    };

    mockGraphsService.getGraph.mockReturnValueOnce({
      err: false,
      val: testGraph,
    });

    const { getByTestId, getByRole } = await customRender(GraphDisplay, {
      props: {
        graphId: mockGraph.id,
      },
      global: {
        provide: defaultProvide,
      },
    });

    await waitFor(() => {
      expect(getByTestId(`graph-display-${testGraph.id}`)).toBeInTheDocument();
    });

    const graph = getByTestId(`graph-${testGraph.id}`);
    expect(graph).toBeInTheDocument();

    const edges = graph.querySelectorAll('.v-ng-edge');
    expect(edges).toHaveLength(2);

    const filter = getByTestId('graph-filter');
    expect(filter).toBeInTheDocument();

    const openFilterButton = getByRole('button', { name: /open filter/i });
    await fireEvent.click(openFilterButton);

    const listItems = within(filter).getAllByRole('listitem');
    const rolesListItem = listItems.find(item => item.textContent?.includes('Roles'));
    expect(rolesListItem).toBeInTheDocument();

    const expandRolesButton = within(rolesListItem!).getByRole('button', { name: /expand/i });
    await fireEvent.click(expandRolesButton);

    const createdByCheckbox = getByRole('checkbox', { name: /created by/i });
    await fireEvent.click(createdByCheckbox);

    const filteredEdges = graph.querySelectorAll('.v-ng-edge');
    expect(filteredEdges).toHaveLength(1);

    await fireEvent.click(createdByCheckbox);

    const unfilteredEdges = graph.querySelectorAll('.v-ng-edge');
    expect(unfilteredEdges).toHaveLength(2);
  });
});

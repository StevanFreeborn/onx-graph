import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, within } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import GraphFilter from '../../src/components/GraphFilter.vue';
import { customRender } from '../testUtils';

describe('GraphFilter', () => {
  afterEach(cleanup);

  it('should display text indicating 0 filters applied', async () => {
    const { getByText } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const filterText = getByText(/filters \(0\)/i);

    expect(filterText).toBeInTheDocument();
  });

  it('should display button to open filter list', async () => {
    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const button = getByRole('button', { name: /open filters/i });

    expect(button).toBeInTheDocument();
  });

  it('should hide filter list by default', async () => {
    const { queryByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const filterList = queryByRole('list');

    expect(filterList).not.toBeInTheDocument();
  });

  it('should display filter list when button is clicked', async () => {
    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const button = getByRole('button', { name: /open filters/i });

    await fireEvent.click(button);

    const filterList = getByRole('list');

    expect(filterList).toBeInTheDocument();
  });

  it('should display a search input in filter list', async () => {
    const { getByRole, getByPlaceholderText } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const searchInput = getByPlaceholderText(/search/i);

    expect(searchInput).toBeInTheDocument();
  });

  it('should filter nodes and edges based on search input', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
      { id: 3, name: 'Roles' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'Users to Groups', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Groups to Users', referencedAppId: 1 }],
      3: [{ id: 3, name: 'Roles to Groups', referencedAppId: 1 }],
    };

    const { getByRole, getByPlaceholderText, getAllByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });
    await fireEvent.click(openButton);

    const expandAllButton = getByRole('button', { name: /expand all/i });
    await fireEvent.click(expandAllButton);

    const searchInput = getByPlaceholderText(/search/i);

    const searchValue = 'User';
    await fireEvent.update(searchInput, searchValue);

    const listItems = getAllByRole('listitem');

    expect(listItems).toHaveLength(4);
  });

  it('should display a select all checkbox in filter list', async () => {
    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const selectAllCheckbox = getByRole('checkbox', { name: /select all/i });

    expect(selectAllCheckbox).toBeInTheDocument();
    expect(selectAllCheckbox).toBeChecked();
  });

  it('should deselect all checkboxes when select all checkbox is unchecked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Group to User', referencedAppId: 1 }],
    };

    const { getByRole, getAllByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const selectAllCheckbox = getByRole('checkbox', { name: /select all/i });

    await fireEvent.click(selectAllCheckbox);

    const listItems = getAllByRole('listitem');

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));
      expect(nodeItem).toBeInTheDocument();

      const { getByRole } = within(nodeItem!);

      const nodeCheckbox = getByRole('checkbox');
      expect(nodeCheckbox).not.toBeChecked();

      const expandButton = getByRole('button', { name: /expand/i });
      await fireEvent.click(expandButton);

      for (const edge of edgesMap[node.id]) {
        const edgeCheckbox = getByRole('checkbox', { name: edge.name });
        expect(edgeCheckbox).not.toBeChecked();
      }
    }
  });

  it('should check all checkboxes when select all checkbox is checked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Group to User', referencedAppId: 1 }],
    };

    const { getByRole, getAllByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const selectAllCheckbox = getByRole('checkbox', { name: /select all/i });

    await fireEvent.click(selectAllCheckbox);
    await fireEvent.click(selectAllCheckbox);

    const listItems = getAllByRole('listitem');

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));
      expect(nodeItem).toBeInTheDocument();

      const { getByRole } = within(nodeItem!);

      const nodeCheckbox = getByRole('checkbox');
      expect(nodeCheckbox).toBeChecked();

      const expandButton = getByRole('button', { name: /expand/i });
      await fireEvent.click(expandButton);

      for (const edge of edgesMap[node.id]) {
        const edgeCheckbox = getByRole('checkbox', { name: edge.name });
        expect(edgeCheckbox).toBeChecked();
      }
    }
  });

  it('should display an expand all button in filter list', async () => {
    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const expandAllButton = getByRole('button', { name: /expand all/i });

    expect(expandAllButton).toBeInTheDocument();
  });

  it('should expand all nodes when expand all button is clicked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Group to User', referencedAppId: 1 }],
    };

    const { getByRole, getAllByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const expandAllButton = getByRole('button', { name: /expand all/i });

    await fireEvent.click(expandAllButton);

    const listItems = getAllByRole('listitem');

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));
      expect(nodeItem).toBeInTheDocument();

      const { getByRole, queryByRole, getAllByRole } = within(nodeItem!);

      const expandButton = queryByRole('button', { name: /expand/i });
      expect(expandButton).not.toBeInTheDocument();

      const collapseButton = getByRole('button', { name: /collapse/i });
      expect(collapseButton).toBeInTheDocument();

      const edgeListItems = getAllByRole('listitem');

      expect(edgeListItems).toHaveLength(edgesMap[node.id].length);
    }
  });

  it('should display a collapse all button in filter list', async () => {
    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const collapseAllButton = getByRole('button', { name: /collapse all/i });

    expect(collapseAllButton).toBeInTheDocument();
  });

  it('should collapse all nodes when collapse all button is clicked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Group to User', referencedAppId: 1 }],
    };

    const { getByRole, getAllByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const expandAllButton = getByRole('button', { name: /expand all/i });

    await fireEvent.click(expandAllButton);

    const collapseAllButton = getByRole('button', { name: /collapse all/i });

    await fireEvent.click(collapseAllButton);

    const listItems = getAllByRole('listitem');

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));
      expect(nodeItem).toBeInTheDocument();

      const { getByRole, queryByRole, queryAllByRole } = within(nodeItem!);

      const expandButton = getByRole('button', { name: /expand/i });
      expect(expandButton).toBeInTheDocument();

      const collapseButton = queryByRole('button', { name: /collapse/i });
      expect(collapseButton).not.toBeInTheDocument();

      const edgeItems = queryAllByRole('listitem');

      expect(edgeItems).toHaveLength(0);
    }
  });

  it('should hide filter list when button is clicked again', async () => {
    const { getByRole, queryByRole } = await customRender(GraphFilter, {
      props: {
        nodes: [],
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const closeButton = getByRole('button', { name: /close filters/i });

    await fireEvent.click(closeButton);

    const filterList = queryByRole('list');

    expect(filterList).not.toBeInTheDocument();
  });

  it('should display a list item for each node in list', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 1, name: 'Groups' },
    ];

    const { getAllByRole, getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const listItems = getAllByRole('listitem');

    expect(listItems).toHaveLength(nodes.length);

    nodes.forEach(node => {
      expect(listItems.some(item => item.textContent?.includes(node.name))).toBe(true);
    });
  });

  it('should display a checked checkbox for each node in list', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 1, name: 'Groups' },
    ];

    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    nodes.forEach(node => {
      const checkbox = within(list).getByLabelText(node.name);

      expect(checkbox).toBeInTheDocument();
      expect(checkbox).toBeChecked();
    });
  });

  it('should increment filter count when node checkbox is unchecked', async () => {
    const nodes = [{ id: 1, name: 'Users' }];

    const { getByRole, getByText } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const checkbox = getByRole('checkbox', { name: /users/i });

    await fireEvent.click(checkbox);

    const filterText = getByText(/filters \(1\)/i);

    expect(filterText).toBeInTheDocument();
  });

  it('should decrement filter count when node checkbox is checked', async () => {
    const nodes = [{ id: 1, name: 'Users' }];

    const { getByRole, getByText } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const checkbox = getByRole('checkbox', { name: /users/i });

    await fireEvent.click(checkbox);

    const incrementedFilterText = getByText(/filters \(1\)/i);

    expect(incrementedFilterText).toBeInTheDocument();

    await fireEvent.click(checkbox);

    const decrementedFilterText = getByText(/filters \(0\)/i);

    expect(decrementedFilterText).toBeInTheDocument();
  });

  it('should not display an expand button for nodes in list with no edges', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [],
      2: [],
    };

    const { getAllByRole, getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const listItems = getAllByRole('listitem');

    expect(listItems).toHaveLength(nodes.length);

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));

      expect(nodeItem).toBeInTheDocument();

      const expandButton = within(nodeItem!).queryByRole('button', { name: /expand/i });

      expect(expandButton).not.toBeInTheDocument();
    }
  });

  it('should display an expand button for nodes in list with edges', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getAllByRole, getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const listItems = getAllByRole('listitem');

    expect(listItems).toHaveLength(nodes.length);

    for (const node of nodes) {
      const nodeItem = listItems.find(item => item.textContent?.includes(node.name));

      expect(nodeItem).toBeInTheDocument();

      const expandButton = within(nodeItem!).queryByRole('button', { name: /expand/i });

      if (edgesMap[node.id].length > 0) {
        expect(expandButton).toBeInTheDocument();
      } else {
        expect(expandButton).not.toBeInTheDocument();
      }
    }
  });

  it('should display a list item for each edge in node when expanded', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    for (const node of nodes) {
      if (edgesMap[node.id].length > 0) {
        const { getByRole, getByText } = within(list);

        const expandButton = getByRole('button', { name: /expand/i });

        await fireEvent.click(expandButton);

        edgesMap[node.id].forEach(edge => {
          const edgeItem = getByText(edge.name);

          expect(edgeItem).toBeInTheDocument();
        });
      }
    }
  });

  it('should display a checked checkbox for each edge in node when expanded', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    for (const node of nodes) {
      if (edgesMap[node.id].length > 0) {
        const expandButton = within(list).getByRole('button', { name: /expand/i });

        await fireEvent.click(expandButton);

        edgesMap[node.id].forEach(edge => {
          const checkbox = within(list).getByLabelText(edge.name);

          expect(checkbox).toBeInTheDocument();
          expect(checkbox).toBeChecked();
        });
      }
    }
  });

  it('should hide edge list when expanded edge list is collapsed', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getByRole } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    for (const node of nodes) {
      if (edgesMap[node.id].length > 0) {
        const { getByRole, queryByRole } = within(list);

        const expandButton = getByRole('button', { name: /expand/i });

        await fireEvent.click(expandButton);

        const edgeList = queryByRole('list');

        expect(edgeList).toBeInTheDocument();

        const collapseButton = getByRole('button', { name: /collapse/i });

        await fireEvent.click(collapseButton);

        expect(edgeList).not.toBeInTheDocument();
      }
    }
  });

  it('should increment filter count when edge checkbox is unchecked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getByRole, getByText } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    for (const node of nodes) {
      if (edgesMap[node.id].length > 0) {
        const { getByRole } = within(list);

        const expandButton = getByRole('button', { name: /expand/i });

        await fireEvent.click(expandButton);

        const edge = edgesMap[node.id][0];

        const checkbox = getByRole('checkbox', { name: edge.name });

        await fireEvent.click(checkbox);

        const filterText = getByText(/filters \(1\)/i);
        expect(filterText).toBeInTheDocument();
      }
    }
  });

  it('should decrement filter count when edge checkbox is checked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [],
    };

    const { getByRole, getByText } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });

    await fireEvent.click(openButton);

    const list = getByRole('list');

    for (const node of nodes) {
      if (edgesMap[node.id].length > 0) {
        const { getByRole } = within(list);

        const expandButton = getByRole('button', { name: /expand/i });

        await fireEvent.click(expandButton);

        const edge = edgesMap[node.id][0];

        const checkbox = getByRole('checkbox', { name: edge.name });

        await fireEvent.click(checkbox);

        const incrementedFilterText = getByText(/filters \(1\)/i);

        expect(incrementedFilterText).toBeInTheDocument();

        await fireEvent.click(checkbox);

        const decrementedFilterText = getByText(/filters \(0\)/i);

        expect(decrementedFilterText).toBeInTheDocument();
      }
    }
  });

  it('should hide and filter node when node checkbox is unchecked', async () => {
    const nodes = [{ id: 1, name: 'Users' }];

    const { getByRole, emitted } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap: {},
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });
    await fireEvent.click(openButton);

    const checkbox = getByRole('checkbox', { name: /users/i });

    await fireEvent.click(checkbox);

    const emits = emitted();
    const filterEvent = emits['filter:node'];
    expect(filterEvent[0]).toEqual([1, false]);
  });

  it("should filter all of node's edges when node checkbox is unchecked", async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
    };

    const { getByRole, emitted } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });
    await fireEvent.click(openButton);

    const checkbox = getByRole('checkbox', { name: /users/i });
    await fireEvent.click(checkbox);

    const emits = emitted();

    const filterNodeEvent = emits['filter:node'];
    expect(filterNodeEvent).toEqual([[1, false]]);

    const filterEvent = emits['filter:edge'];
    expect(filterEvent).toEqual([[1, 1, false]]);
  });

  it('should filter all edges which reference node when node checkbox is unchecked', async () => {
    const nodes = [
      { id: 1, name: 'Users' },
      { id: 2, name: 'Groups' },
    ];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
      2: [{ id: 2, name: 'Group to User', referencedAppId: 1 }],
    };

    const { getByRole, emitted } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });
    await fireEvent.click(openButton);

    const checkbox = getByRole('checkbox', { name: /users/i });
    await fireEvent.click(checkbox);

    const emits = emitted();
    const filterNodeEvents = emits['filter:node'];
    expect(filterNodeEvents).toEqual([[1, false]]);

    const filterEdgeEvents = emits['filter:edge'];
    expect(filterEdgeEvents).toEqual([
      [1, 1, false],
      [2, 2, false],
    ]);
  });

  it('should filter edge when edge checkbox is unchecked', async () => {
    const nodes = [{ id: 1, name: 'Users' }];

    const edgesMap = {
      1: [{ id: 1, name: 'User to Group', referencedAppId: 2 }],
    };

    const { getByRole, emitted } = await customRender(GraphFilter, {
      props: {
        nodes,
        edgesMap,
      },
    });

    const openButton = getByRole('button', { name: /open filters/i });
    await fireEvent.click(openButton);

    const expandAllButton = getByRole('button', { name: /expand all/i });
    await fireEvent.click(expandAllButton);

    const checkbox = getByRole('checkbox', { name: /user to group/i });
    await fireEvent.click(checkbox);

    const emits = emitted();
    const filterEvent = emits['filter:edge'];
    expect(filterEvent).toEqual([[1, 1, false]]);
  });
});

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
});

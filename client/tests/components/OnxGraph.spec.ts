import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import OnxGraph from '../../src/components/OnxGraph.vue';
import { customRender } from '../testUtils';

describe('OnxGraph', () => {
  const testGraphWithLayout = {
    id: '1',
    userId: '1',
    name: 'With Layout',
    createdAt: '2024-05-30T23:58:10.197Z',
    updatedAt: '2024-05-31T20:49:26.335Z',
    status: 2,
    nodes: [
      { href: null, id: 2, name: 'Users' },
      { href: null, id: 1, name: 'Roles' },
      { href: null, id: 3, name: 'Groups' },
    ],
    edgesMap: {
      '1': [
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 31,
          appId: 1,
          name: 'Users',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 41,
          appId: 1,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 48,
          appId: 1,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 23,
          appId: 1,
          name: 'Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
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
      '2': [
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
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 895,
          appId: 2,
          name: 'Manager',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 50,
          appId: 2,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 20,
          appId: 2,
          name: 'Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 896,
          appId: 2,
          name: 'Direct Reports',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 42,
          appId: 2,
          name: 'Created By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 39,
          appId: 2,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
      ],
      '3': [
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 21,
          appId: 3,
          name: 'Users',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 40,
          appId: 3,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 1,
          id: 22,
          appId: 3,
          name: 'Roles',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 18,
          appId: 3,
          name: 'Parent Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 52,
          appId: 3,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 43,
          appId: 3,
          name: 'Created By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 19,
          appId: 3,
          name: 'Child Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
      ],
    },
    layout: {
      '1': { x: 76.498, y: -61.225 },
      '2': { x: 62.765, y: -63.055 },
      '3': { x: 69.32, y: -51.981 },
    },
  };

  const testGraphWithoutLayout = {
    id: '1',
    userId: '1',
    name: 'Without Layout',
    createdAt: '2024-05-30T23:58:10.197Z',
    updatedAt: '2024-05-31T20:49:26.335Z',
    status: 2,
    nodes: [
      { id: 2, name: 'Users' },
      { id: 1, name: 'Roles' },
      { id: 3, name: 'Groups' },
    ],
    edgesMap: {
      '1': [
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 31,
          appId: 1,
          name: 'Users',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 41,
          appId: 1,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 48,
          appId: 1,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 23,
          appId: 1,
          name: 'Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
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
      '2': [
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
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 895,
          appId: 2,
          name: 'Manager',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 50,
          appId: 2,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 20,
          appId: 2,
          name: 'Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 896,
          appId: 2,
          name: 'Direct Reports',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 42,
          appId: 2,
          name: 'Created By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 39,
          appId: 2,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
      ],
      '3': [
        {
          multiplicity: 1,
          referencedAppId: 2,
          id: 21,
          appId: 3,
          name: 'Users',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 40,
          appId: 3,
          name: 'Updated By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 1,
          id: 22,
          appId: 3,
          name: 'Roles',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 18,
          appId: 3,
          name: 'Parent Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 52,
          appId: 3,
          name: 'Last Saved By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 0,
          referencedAppId: 2,
          id: 43,
          appId: 3,
          name: 'Created By',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
        {
          multiplicity: 1,
          referencedAppId: 3,
          id: 19,
          appId: 3,
          name: 'Child Groups',
          type: 500,
          status: 0,
          isRequired: false,
          isUnique: false,
        },
      ],
    },
    layout: null,
  };

  afterEach(cleanup);

  for (const graph of [testGraphWithLayout, testGraphWithoutLayout]) {
    it(`should render graph with ${graph.layout ? 'layout' : 'no layout'}`, async () => {
      const { getByTestId } = await customRender(OnxGraph, {
        props: { graph: testGraphWithLayout },
      });

      const graph = getByTestId(`graph-${testGraphWithLayout.id}`);
      expect(graph).toBeInTheDocument();

      const nodesLayer = graph.querySelector('.v-ng-layer-nodes');
      expect(nodesLayer).toBeInTheDocument();

      const nodes = nodesLayer!.querySelectorAll('.v-ng-node');
      expect(nodes).toHaveLength(testGraphWithLayout.nodes.length);

      const edgesLayer = graph.querySelector('.v-ng-layer-edges');
      expect(edgesLayer).toBeInTheDocument();

      const edges = edgesLayer!.querySelectorAll('.v-ng-line-background');
      const expectedEdges = Object.values(testGraphWithLayout.edgesMap).flat().length;
      expect(edges).toHaveLength(expectedEdges);
    });
  }

  it('should hide explorer when rendered', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const explorer = getByTestId('explorer');
    expect(explorer.style.display).toBe('none');
  });

  it('should display explorer when node is clicked', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const nodes = getByTestId(`graph-${testGraphWithLayout.id}`).querySelectorAll('.v-ng-node');
    const node = nodes[0];
    await fireEvent.click(node);

    const explorer = getByTestId('explorer');
    expect(explorer.style.display).not.toBe('none');
  });

  it('should display explorer when edge is clicked', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const edges = getByTestId(`graph-${testGraphWithLayout.id}`).querySelectorAll(
      '.v-ng-line-background'
    );
    const edge = edges[0];
    await fireEvent.click(edge);

    const explorer = getByTestId('explorer');
    expect(explorer.style.display).not.toBe('none');
  });

  it('should display only edges in explorer for node clicked', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const graph = getByTestId(`graph-${testGraphWithLayout.id}`);
    const nodes = graph.querySelectorAll('.v-ng-node');
    const labels = graph.querySelectorAll('.v-ng-node-label');

    const node = nodes[0];
    const nodeName = labels[0].textContent;

    await fireEvent.click(node);

    const selectedNode = testGraphWithLayout.nodes.find(n => n.name === nodeName);

    expect(selectedNode).toBeDefined();

    const explorer = getByTestId('explorer');

    const explorerHeaderRows = explorer.querySelectorAll('thead > tr > th');
    const sourceAppNameIndex = [...explorerHeaderRows].findIndex(
      row => row.textContent === 'Source App Name'
    );

    const edgeRows = explorer.querySelectorAll('tbody > tr');

    for (const edgeRow of edgeRows) {
      const sourceAppName = edgeRow.querySelectorAll('td')[sourceAppNameIndex].textContent;
      expect(sourceAppName).toBe(selectedNode!.name);
    }
  });

  it('should display only edge in explorer for edge clicked', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const graph = getByTestId(`graph-${testGraphWithLayout.id}`);

    const edges = graph.querySelectorAll('.v-ng-line-background');
    const edge = edges[0];

    await fireEvent.click(edge);

    const explorer = getByTestId('explorer');

    const explorerHeaderRows = explorer.querySelectorAll('thead > tr > th');
    const fieldNameIndex = [...explorerHeaderRows].findIndex(
      row => row.textContent === 'Field Name'
    );

    const edgeRows = explorer.querySelectorAll('tbody > tr');
    expect(edgeRows).toHaveLength(1);

    const edgeRow = edgeRows[0];
    const fieldName = edgeRow.querySelectorAll('td')[fieldNameIndex].textContent;

    const graphEdges = Object.values(testGraphWithLayout.edgesMap).flat();

    const selectedEdge = graphEdges.find(e => e.name === fieldName);

    expect(selectedEdge).toBeDefined();
  });

  it('should show all edges in explorer when background is clicked and explorer is open', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const graph = getByTestId(`graph-${testGraphWithLayout.id}`);
    const nodes = graph.querySelectorAll('.v-ng-node');
    const labels = graph.querySelectorAll('.v-ng-node-label');

    const node = nodes[0];
    const nodeName = labels[0].textContent;

    await fireEvent.click(node);

    const selectedNode = testGraphWithLayout.nodes.find(n => n.name === nodeName);

    expect(selectedNode).toBeDefined();

    const explorer = getByTestId('explorer');

    const explorerHeaderRows = explorer.querySelectorAll('thead > tr > th');
    const sourceAppNameIndex = [...explorerHeaderRows].findIndex(
      row => row.textContent === 'Source App Name'
    );
    const fieldNameIndex = [...explorerHeaderRows].findIndex(
      row => row.textContent === 'Field Name'
    );

    const edgeRows = explorer.querySelectorAll('tbody > tr');

    for (const edgeRow of edgeRows) {
      const sourceAppName = edgeRow.querySelectorAll('td')[sourceAppNameIndex].textContent;
      expect(sourceAppName).toBe(selectedNode!.name);
    }

    const background = graph.querySelector('.v-ng-canvas');
    expect(background).toBeInTheDocument();

    await fireEvent.click(background!);

    const explorerRows = explorer.querySelectorAll('tbody > tr');
    const edges = Object.values(testGraphWithLayout.edgesMap).flat();

    expect(explorerRows).toHaveLength(edges.length);

    for (const edge of edges) {
      const edgeRow = [...explorerRows].find(row => {
        const fieldName = row.querySelectorAll('td')[fieldNameIndex].textContent;
        return fieldName === edge.name;
      });

      expect(edgeRow).toBeDefined();
    }
  });

  it('should hide explorer when background is double clicked', async () => {
    const { getByTestId } = await customRender(OnxGraph, {
      props: { graph: testGraphWithLayout },
    });

    const graph = getByTestId(`graph-${testGraphWithLayout.id}`);
    const background = graph.querySelector('.v-ng-canvas');
    const explorer = getByTestId('explorer');
    const nodes = graph.querySelectorAll('.v-ng-node');
    const node = nodes[0];

    expect(background).toBeInTheDocument();

    await fireEvent.click(node);

    expect(explorer.style.display).not.toBe('none');

    await fireEvent.dblClick(background!);

    expect(explorer.style.display).toBe('none');
  });
});

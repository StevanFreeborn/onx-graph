<script setup lang="ts">
  import { useTheme } from '@/composables/useTheme';
  import type { Graph, GraphLayout } from '@/types';
  import * as vNG from 'v-network-graph';
  import { ForceLayout } from 'v-network-graph/lib/force-layout';
  import { computed, ref, watch, type ComputedRef } from 'vue';

  const props = defineProps<{
    graph: Graph;
  }>();

  const emit = defineEmits<{
    'update:layout': [layout: GraphLayout];
  }>();

  const graphLayout = ref(props.graph.layout);
  const selectedNode = ref<number | null>(null);
  const selectedEdges = ref<number[]>([]);

  const theme = useTheme();

  const configs: ComputedRef<vNG.Config> = computed(() =>
    vNG.defineConfigs({
      view: {
        doubleClickZoomEnabled: false,
        autoPanAndZoomOnLoad: 'fit-content',
        layoutHandler: props.graph.layout
          ? undefined
          : new ForceLayout({
              positionFixedByDrag: false,
              positionFixedByClickWithAltKey: false,
              noAutoRestartSimulation: true,

              createSimulation: (d3, nodes) => {
                return d3
                  .forceSimulation(nodes)
                  .force('charge', d3.forceManyBody().strength(-2000))
                  .force('x', d3.forceX())
                  .force('y', d3.forceY())
                  .stop()
                  .tick(100);
              },
            }),
      },
      node: {
        selectable: true,
        normal: {
          type: 'rect',
          width: 32,
          height: 32,
          borderRadius: 8,
          color: '#f8951b',
        },
        hover: {
          color: '#f8951b',
          width: 36,
          height: 36,
          borderRadius: 8,
        },
        focusring: {
          visible: true,
          width: 4,
          padding: 3,
          color: theme.value === 'dark' ? '#ebebeba3' : '#2c3e50',
          dasharray: '0',
        },
        label: {
          fontSize: 16,
          color: theme.value === 'dark' ? '#ebebeba3' : '#2c3e50',
          direction: 'north',
          margin: 8,
          visible: true,
          directionAutoAdjustment: true,
        },
      },
      edge: {
        type: 'straight',
        gap: 5,
        selectable: true,
        normal: {
          width: 2,
          color: '#f8951b',
          dasharray: '4 6',
          linecap: 'round',
        },
        hover: {
          color: '#f8951b',
        },
        marker: {
          source: {
            type: 'none',
            width: 4,
            height: 4,
            margin: -1,
            offset: 0,
            units: 'strokeWidth',
            color: null,
          },
          target: {
            type: 'arrow',
            width: 4,
            height: 4,
            margin: -1,
            offset: 0,
            units: 'strokeWidth',
            color: null,
          },
        },
        summarized: {
          selectable: true,
          stroke: {
            color: '#f8951b',
            width: 2,
          },
          label: {
            fontSize: 10,
            color: theme.value === 'dark' ? '#ebebeba3' : '#2c3e50',
          },
          shape: {
            type: 'rect',
            width: 16,
            height: 16,
            borderRadius: 3,
            color: theme.value === 'dark' ? '#222222' : '#f8f8f8',
            strokeWidth: 1,
            strokeColor: '#f8951b',
            strokeDasharray: undefined,
          },
        },
        selfLoop: {
          radius: 14,
          offset: 16,
          angle: 180,
          isClockwise: true,
        },
      },
    })
  );

  const nodes = computed(() =>
    props.graph.nodes.reduce(
      (nodes, node) => {
        nodes[`node${node.id}`] = {
          name: node.name,
        };

        return nodes;
      },
      {} as Record<string, any>
    )
  );

  // reset selectedNode when the node is not found in the graph
  watch(nodes, () => {
    const node = props.graph.nodes.find(node => node.id === selectedNode.value);

    if (node !== undefined) {
      return;
    }

    selectedNode.value = null;
  });

  const edges = computed(() =>
    Object.keys(props.graph.edgesMap).reduce(
      (edges, edgeApp) => {
        const currentEdges = props.graph.edgesMap[edgeApp];

        for (const edge of currentEdges) {
          edges[`edge${edge.id}`] = {
            source: `node${edge.appId}`,
            target: `node${edge.referencedAppId}`,
          };
        }

        return edges;
      },
      {} as Record<string, any>
    )
  );

  // reset selectedEdges when the edge is not found in the graph
  watch(edges, () => {
    selectedEdges.value = selectedEdges.value.filter(edgeId => {
      for (const key in props.graph.edgesMap) {
        const currentEdges = props.graph.edgesMap[key];
        const foundEdge = currentEdges.find(edge => edge.id === edgeId);
        const foundSourceNode = props.graph.nodes.find(node => node.id === foundEdge?.appId);
        const foundTargetNode = props.graph.nodes.find(
          node => node.id === foundEdge?.referencedAppId
        );

        if (foundEdge && foundSourceNode && foundTargetNode) {
          return true;
        }
      }

      return false;
    });
  });

  const explorerTableColumns = [
    { key: 'sourceAppId', label: 'Source App Id' },
    { key: 'sourceAppName', label: 'Source App Name' },
    { key: 'targetAppId', label: 'Target App Id' },
    { key: 'targetAppName', label: 'Target App Name' },
    { key: 'fieldId', label: 'Field Id' },
    { key: 'fieldName', label: 'Field Name' },
    { key: 'multiplicity', label: 'Multiplicity' },
  ];

  const explorerTableRows = computed(() => {
    const nodes = props.graph.nodes;
    const edgesMap = props.graph.edgesMap;

    const rows = [];

    const edgesKeys = Object.keys(edgesMap);

    for (const key of edgesKeys) {
      const keyAsInt = parseInt(key);

      if (Number.isNaN(keyAsInt)) {
        continue;
      }

      const edgeNode = nodes.find(node => node.id === keyAsInt);

      const edges = edgesMap[key];

      for (const edge of edges) {
        const referencedNode = nodes.find(node => node.id === edge.referencedAppId);
        const multiplicity = edge.multiplicity === 1 ? 'Multi Select' : 'Single Select';

        if (edgeNode === undefined || referencedNode === undefined) {
          continue;
        }

        if (selectedNode.value !== null && selectedNode.value !== edgeNode.id) {
          continue;
        }

        if (selectedEdges.value.length > 0 && !selectedEdges.value.includes(edge.id)) {
          continue;
        }

        const row = {
          sourceAppId: edgeNode.id,
          sourceAppName: edgeNode.name,
          targetAppId: referencedNode.id,
          targetAppName: referencedNode.name,
          fieldId: edge.id,
          fieldName: edge.name,
          multiplicity: multiplicity,
        };

        rows.push(row);
      }
    }

    rows.sort((a, b) => {
      if (a.sourceAppName === undefined || b.sourceAppName === undefined) {
        return 0;
      }

      return a.sourceAppName.localeCompare(b.sourceAppName);
    });

    return rows;
  });

  function getGraphLayout(layouts: vNG.Layouts) {
    return Object.keys(layouts.nodes).reduce((layout, node) => {
      const nodeId = node.replace('node', '');
      const currentNode = layouts.nodes[node];

      layout[nodeId] = {
        x: currentNode.x,
        y: currentNode.y,
      };

      return layout;
    }, {} as GraphLayout);
  }

  const layouts = computed({
    get() {
      return props.graph.layout
        ? Object.keys(props.graph.layout).reduce(
            (layouts, node) => {
              const currentNode = props.graph.layout[node];

              layouts.nodes[`node${node}`] = {
                x: currentNode.x,
                y: currentNode.y,
              };

              return layouts;
            },
            { nodes: {} as Record<string, any> }
          )
        : { nodes: {} };
    },
    set(newValue) {
      graphLayout.value = getGraphLayout(newValue);
    },
  });

  const containerRef = ref<HTMLElement | null>(null);
  const explorerContainerDisplay = ref(false);
  const explorerContainerHeight = ref(0);
  const explorerContainerStyles = computed(() => ({
    height: `${explorerContainerHeight.value}px`,
    display: explorerContainerDisplay.value ? 'flex' : 'none',
  }));

  function showExplorer() {
    explorerContainerDisplay.value = true;

    if (explorerContainerHeight.value > 0) {
      return;
    }

    const containerHeight = containerRef.value?.clientHeight ?? 0;
    const newExplorerContainerHeight = containerHeight === 0 ? 0 : containerHeight / 2;
    explorerContainerHeight.value = newExplorerContainerHeight;
  }

  function hideExplorer() {
    explorerContainerDisplay.value = false;
  }

  const eventHandlers: vNG.EventHandlers = {
    'node:dragend': () => {
      emit('update:layout', graphLayout.value);
    },
    'node:click': nodeEvent => {
      selectedNode.value = parseInt(nodeEvent.node.replace('node', ''));
      selectedEdges.value = [];
      showExplorer();
    },
    'edge:click': edgeEvent => {
      selectedNode.value = null;

      if (edgeEvent.edge !== undefined) {
        selectedEdges.value = [parseInt(edgeEvent.edge.replace('edge', ''))];
      }

      if (edgeEvent.edges !== undefined) {
        selectedEdges.value = edgeEvent.edges.map(edge => parseInt(edge.replace('edge', '')));
      }

      showExplorer();
    },
    'view:click': () => {
      selectedNode.value = null;
      selectedEdges.value = [];
    },
    'view:dblclick': () => {
      hideExplorer();
    },
  };

  function handleMove(containerHeight: number, newExplorerContainerHeight: number) {
    if (newExplorerContainerHeight > containerHeight) {
      explorerContainerHeight.value = containerHeight;
      return;
    }

    if (newExplorerContainerHeight < 10) {
      explorerContainerHeight.value = 10;
      return;
    }

    explorerContainerHeight.value = newExplorerContainerHeight;
  }

  function handleMouseMove(event: MouseEvent) {
    const containerHeight = containerRef.value?.clientHeight ?? 0;
    const newExplorerContainerHeight = containerHeight - event.clientY + 20;
    handleMove(containerHeight, newExplorerContainerHeight);
  }

  function handleMouseDown(event: MouseEvent) {
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('mouseup', handleMouseUp);
    event.preventDefault();
  }

  function handleMouseUp(event: MouseEvent) {
    window.removeEventListener('mousemove', handleMouseMove);
    window.removeEventListener('mouseup', handleMouseUp);
    event.preventDefault();
  }

  function handleTouchMove(event: TouchEvent) {
    const touch = event.touches[0];
    const containerHeight = containerRef.value?.clientHeight ?? 0;
    const newExplorerContainerHeight = containerHeight - touch.clientY + 20;
    handleMove(containerHeight, newExplorerContainerHeight);
  }

  function handleTouchStart(event: TouchEvent) {
    window.addEventListener('touchmove', handleTouchMove);
    window.addEventListener('touchend', handleTouchEnd);
    event.preventDefault();
  }

  function handleTouchEnd(event: TouchEvent) {
    window.removeEventListener('touchmove', handleTouchMove);
    window.removeEventListener('touchend', handleTouchEnd);
    event.preventDefault();
  }
</script>

<template>
  <div class="container" ref="containerRef">
    <v-network-graph
      class="graph"
      :nodes="nodes"
      :edges="edges"
      v-model:layouts="layouts"
      :configs="configs"
      :eventHandlers="eventHandlers"
    />
    <div class="explorer-container" :style="explorerContainerStyles">
      <div
        class="drag-bar"
        @mousedown="handleMouseDown"
        @mouseup="handleMouseUp"
        @touchstart="handleTouchStart"
        @touchend="handleTouchEnd"
      >
        <div class="handle"></div>
      </div>
      <div class="explorer-table-container">
        <table>
          <thead>
            <tr>
              <th v-for="column in explorerTableColumns" :key="column.key" :title="column.label">
                {{ column.label }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in explorerTableRows" :key="row.fieldId">
              <td
                v-for="[index, cell] in Object.values(row).entries()"
                :key="`${row.fieldId}-${index}`"
                title="cell"
              >
                {{ cell }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    width: 100%;
    height: 100%;
    flex: 1;
    position: relative;

    & .graph {
      display: flex;
      width: 100%;
      height: 100%;
    }

    & .explorer-container {
      z-index: 2;
      flex-direction: column;
      position: absolute;
      width: 100%;
      bottom: 0;
      background-color: var(--color-background-mute);

      & .explorer-table-container {
        overflow-y: auto;
        scrollbar-color: var(--color-background) var(--color-background-mute);
        height: 100%;
        padding: 1rem;
        display: flex;
        flex-direction: column;
        gap: 0.5rem;

        & table {
          width: 100%;
          border-collapse: collapse;

          & thead {
            background-color: var(--color-background-soft);
          }

          & th {
            padding: 0.5rem;
            text-align: left;
            border-bottom: 1px solid var(--color-background);

            &:first-child {
              border-top-left-radius: 0.25rem;
            }

            &:last-child {
              border-top-right-radius: 0.25rem;
            }
          }

          & td {
            padding: 0.5rem;
            border-bottom: 1px solid var(--color-background);
          }

          & th,
          & td {
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }

          & tr:hover {
            background-color: var(--color-background-soft);
          }
        }
      }

      & .drag-bar {
        display: flex;
        justify-content: center;
        align-items: center;
        width: 100%;
        height: 10px;
        background-color: var(--color-background-soft);
        cursor: ns-resize;

        .handle {
          width: 20px;
          height: 10px;
          background-color: var(--color-background);
          cursor: ns-resize;
          border-radius: 0.25rem;
        }
      }
    }
  }
</style>

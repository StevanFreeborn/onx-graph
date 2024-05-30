<script setup lang="ts">
  import { useTheme } from '@/composables/useTheme';
  import type { Graph, GraphLayout } from '@/types';
  import * as vNG from 'v-network-graph';
  import { ForceLayout } from 'v-network-graph/lib/force-layout';
  import { computed, watch, type ComputedRef, type WritableComputedRef } from 'vue';

  const props = defineProps<{
    graph: Graph;
  }>();

  const emit = defineEmits<{
    'update:layout': [layout: GraphLayout];
  }>();

  const theme = useTheme();

  const configs: ComputedRef<vNG.Config> = computed(() =>
    vNG.defineConfigs({
      view: {
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

  const nodes: ComputedRef<vNG.Nodes> = computed(() =>
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

  const edges: ComputedRef<vNG.Edges> = computed(() =>
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

  const layouts: WritableComputedRef<vNG.Layouts> = computed({
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
      return newValue;
    },
  });

  const eventHandlers: vNG.EventHandlers = {
    'node:dragend': () => {
      const layout = getGraphLayout(layouts.value);
      emit('update:layout', layout);
    },
  };

  // handle initial layout being
  // generated for the graph
  if (props.graph.layout === null) {
    watch(layouts, newValue => {
      const layout = getGraphLayout(newValue);
      emit('update:layout', layout);
    });
  }
</script>

<template>
  <div class="container">
    <v-network-graph
      class="graph"
      :nodes="nodes"
      :edges="edges"
      v-model:layouts="layouts"
      :configs="configs"
      :eventHandlers="eventHandlers"
    />
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    width: 100%;
    height: 100%;
    flex: 1;
  }

  .graph {
    display: flex;
    width: 100%;
    height: 100%;
  }
</style>

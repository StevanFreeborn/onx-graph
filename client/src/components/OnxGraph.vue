<script setup lang="ts">
  import { useTheme } from '@/composables/useTheme';
  import type { Graph } from '@/types';
  import * as vNG from 'v-network-graph';
  import {
    ForceLayout,
    type ForceEdgeDatum,
    type ForceNodeDatum,
  } from 'v-network-graph/lib/force-layout';
  import { computed } from 'vue';

  const props = defineProps<{
    graph: Graph;
  }>();

  const theme = useTheme();

  const configs = computed(() =>
    vNG.defineConfigs({
      view: {
        autoPanAndZoomOnLoad: 'fit-content',
        layoutHandler: new ForceLayout({
          positionFixedByDrag: false,
          positionFixedByClickWithAltKey: false,
          noAutoRestartSimulation: true,

          createSimulation: (d3, nodes, edges) => {
            const forceLink = d3
              .forceLink<ForceNodeDatum, ForceEdgeDatum>(edges)
              .id((d: ForceNodeDatum) => d.id);

            return d3
              .forceSimulation(nodes)
              .force('edge', forceLink.distance(10).strength(2))
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
</script>

<template>
  <div class="container">
    <v-network-graph class="graph" :nodes="nodes" :edges="edges" :configs="configs" />
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

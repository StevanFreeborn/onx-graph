<script setup lang="ts">
  import { useTheme } from '@/composables/useTheme';
  import { computed } from 'vue';

  const theme = useTheme();

  const configs = computed(() => ({
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
      label: {
        fontSize: 16,
        color: theme.value === 'dark' ? '#ebebeba3' : '#2c3e50',
        direction: 'north',
      },
    },
    edge: {
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
    },
  }));

  const nodes = {
    node1: { name: 'Node 1' },
    node2: { name: 'Node 2' },
    node3: { name: 'Node 3' },
    node4: { name: 'Node 4' },
  };

  const edges = {
    edge1: { source: 'node1', target: 'node2' },
    edge2: { source: 'node2', target: 'node3' },
    edge3: { source: 'node3', target: 'node4' },
  };
</script>

<template>
  <main>
    <v-network-graph
      class="graph"
      :nodes="nodes"
      :edges="edges"
      :configs="configs"
    />
  </main>
</template>

<style scoped>
  main {
    display: flex;
    width: 100%;
    flex: 1;
  }

  .graph {
    display: flex;
    width: 100%;
    height: 100%;
    background-color: var(--color-background-soft);
    border-radius: 1rem;
  }

  @media (min-width: 1024px) {
    main {
      height: 500px;
    }
  }
</style>

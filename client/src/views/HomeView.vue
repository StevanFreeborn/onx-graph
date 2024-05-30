<script setup lang="ts">
  import OnxGraph from '@/components/OnxGraph.vue';
  import { exampleGraph } from '../assets/homeGraph';
</script>

<template>
  <main>
    <OnxGraph :graph="exampleGraph" data-testid="demo-graph" />
  </main>
</template>

<style scoped>
  main {
    display: flex;
    width: 100%;
    flex: 1;
    background-color: var(--color-background-soft);
    border-radius: 0.5rem;
  }

  @media (min-width: 1024px) {
    main {
      height: 500px;
    }
  }
</style>

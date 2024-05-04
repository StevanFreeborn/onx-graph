<script setup lang="ts">
  import OnxGraph from '@/components/OnxGraph.vue';
  import { exampleGraph } from '../assets/homeGraph';
</script>

<template>
  <main>
    <OnxGraph :graph="exampleGraph" />
  </main>
</template>

<style scoped>
  main {
    display: flex;
    width: 100%;
    flex: 1;
  }

  @media (min-width: 1024px) {
    main {
      height: 500px;
    }
  }
</style>

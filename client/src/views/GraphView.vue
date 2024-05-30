<script setup lang="ts">
  import GraphDisplay from '@/components/GraphDisplay.vue';
  import { useRoute } from 'vue-router';

  const route = useRoute();
  const id = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id;
</script>

<template>
  <div class="container">
    <GraphDisplay :graph-id="id" />
  </div>
</template>

<style scoped>
  .container {
    position: relative;
    height: 100%;
    width: 100%;
    background-color: var(--color-background-mute);
    border-radius: 0.5rem;
    overflow: hidden;
  }
</style>

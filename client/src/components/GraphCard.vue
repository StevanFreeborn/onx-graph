<script setup lang="ts">
  import { computed } from 'vue';
  import type { Graph } from '../types.js';

  const props = defineProps<{
    graph: Graph;
  }>();

  function formatDate(date: string) {
    return new Date(date).toLocaleString(undefined, {
      month: 'numeric',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      hour12: true,
    });
  }

  var createdAt = computed(() => formatDate(props.graph.createdAt));
  var updatedAt = computed(() => formatDate(props.graph.updatedAt));
</script>

<template>
  <div class="card">
    <RouterLink :to="`/graphs/${graph.id}`" :title="graph.name">
      {{ graph.name }}
    </RouterLink>
    <div class="details">
      <div>Created: {{ createdAt }}</div>
      <div>Updated: {{ updatedAt }}</div>
    </div>
  </div>
</template>

<style scoped>
  .card {
    background-color: var(--color-background-mute);
    padding: 1rem;
    border-radius: 0.5rem;
    box-shadow: 0 0 1rem rgba(0, 0, 0, 0.1);

    & a {
      display: block;
      width: 100%;
      color: var(--color-text);
      font-size: 1.25rem;
      padding: 0.25rem 1rem;
      border-radius: 0.5rem;
      text-wrap: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    @media (hover: hover) {
      & a:hover {
        color: var(--orange);
      }
    }

    & .details {
      padding: 0 1rem;
      color: var(--color-text-mute);
      margin-bottom: 0.25rem;
    }
  }
</style>

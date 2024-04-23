<script setup lang="ts">
  const graphs = [] as { id: number; name: string }[];
</script>

<template>
  <h2>Graphs</h2>
  <div v-if="graphs.length === 0" class="placeholder-container">
    <p>You don't have any graphs yet. Click the button below to add a graph.</p>
    <RouterLink to="/graphs/add" class="button">Add Graph</RouterLink>
  </div>
  <div v-else>
    <ul>
      <li v-for="graph in graphs" :key="graph.id">
        <RouterLink :to="`/graphs/${graph.id}`">{{ graph.name }}</RouterLink>
      </li>
    </ul>
  </div>
</template>

<style scoped>
  .placeholder-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    gap: 1rem;

    & .button {
      background-color: var(--color-background-mute);
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
      border-width: 2px;
      border-style: outset;
      border-color: buttonborder;
      border-image: initial;
      color: var(--color-text);
    }

    & .button:active {
      border-style: inset;
    }

    & .button:hover {
      color: var(--orange);
    }
  }
</style>

<script setup lang="ts">
  import DataPager from '@/components/DataPager.vue';
  import GraphCard from '@/components/GraphCard.vue';
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import { useGraphsService } from '@/composables/useGraphsService';
  import { useUserStore } from '@/stores/userStore';
  import type { Graph, PageWithData } from '@/types';
  import { onMounted, ref } from 'vue';

  const graphsPage = ref<PageWithData<Graph> | null>(null);

  const userStore = useUserStore();
  const graphService = useGraphsService(userStore);

  async function getGraphs(pageNumber = 1) {
    const graphsResult = await graphService.getGraphs(pageNumber);

    if (graphsResult.err) {
      for (const error of graphsResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }
      // TODO: need to have an error ui to show the user
      return;
    }

    graphsPage.value = graphsResult.val;
  }

  onMounted(async () => await getGraphs());

  async function handlePreviousPage() {
    if (graphsPage.value === null || graphsPage.value.pageNumber === 1) {
      return;
    }

    await getGraphs(graphsPage.value.pageNumber - 1);
  }

  async function handleNextPage() {
    if (graphsPage.value === null || graphsPage.value.pageNumber === graphsPage.value.totalPages) {
      return;
    }

    await getGraphs(graphsPage.value.pageNumber + 1);
  }
</script>

<template>
  <h2>Graphs</h2>
  <Transition mode="out-in">
    <div v-if="graphsPage === null" class="loader-container">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphsPage.pageCount === 0" class="placeholder-container">
      <p>You don't have any graphs yet. Click the button below to add a graph.</p>
      <RouterLink to="/graphs/add" class="button">Add Graph</RouterLink>
    </div>
    <div v-else class="graphs-container">
      <ul>
        <li v-for="graph in graphsPage.data" :key="graph.id">
          <GraphCard :graph="graph" />
        </li>
      </ul>
      <DataPager @previous="handlePreviousPage" @next="handleNextPage" :page="graphsPage" />
    </div>
  </Transition>
</template>

<style scoped>
  h2 {
    margin-bottom: 1rem;
  }

  .v-enter-active,
  .v-leave-active {
    transition: opacity 0.5s ease-out;
  }

  .v-enter-from {
    opacity: 100;
  }

  .v-leave-to {
    opacity: 0;
  }

  .loader-container {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100%;
  }

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

  .graphs-container {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 1rem;
    flex: 1;

    & ul {
      display: grid;
      gap: 1rem;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
    }
  }
</style>

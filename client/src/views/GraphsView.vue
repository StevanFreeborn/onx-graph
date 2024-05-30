<script setup lang="ts">
  import DataPager from '@/components/DataPager.vue';
  import GraphCard from '@/components/GraphCard.vue';
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import { useGraphsService } from '@/composables/useGraphsService';
  import { useMounted } from '@/composables/useMounted.js';
  import { useUserStore } from '@/stores/userStore';
  import type { Graph, PageWithData } from '@/types';
  import { onMounted, ref } from 'vue';

  type GraphData =
    | {
        status: 'loaded';
        data: PageWithData<Graph>;
      }
    | {
        status: 'loading';
      }
    | {
        status: 'error';
      };

  const graphs = ref<GraphData>({ status: 'loading' });

  const userStore = useUserStore();
  const graphService = useGraphsService(userStore);
  const isMounted = useMounted();

  async function getGraphs(pageNumber = 1) {
    if (graphs.value.status === 'error') {
      graphs.value = { status: 'loading' };
    }

    const graphsResult = await graphService.getGraphs(pageNumber);

    if (graphsResult.err) {
      for (const error of graphsResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      graphs.value = { status: 'error' };
      return;
    }

    graphs.value = { status: 'loaded', data: graphsResult.val };
  }

  onMounted(async () => await getGraphs());

  async function handlePreviousPage() {
    if (graphs.value.status !== 'loaded') {
      return;
    }

    await getGraphs(graphs.value.data.pageNumber - 1);
  }

  async function handleNextPage() {
    if (graphs.value.status !== 'loaded') {
      return;
    }

    await getGraphs(graphs.value.data.pageNumber + 1);
  }
</script>

<template>
  <div class="heading-container">
    <h2>Graphs</h2>
    <Transition mode="out-in">
      <RouterLink
        v-if="graphs.status === 'loaded' && graphs.data.pageCount > 0"
        to="/graphs/add"
        class="button"
      >
        Add Graph
      </RouterLink>
      <div v-else></div>
    </Transition>
  </div>
  <Transition mode="out-in">
    <div v-if="graphs.status === 'loading'" class="loader-container">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphs.status === 'error'" class="placeholder-container">
      <p>There was an error loading your graphs. Please try again later.</p>
      <button
        :disabled="isMounted === false"
        @click="() => getGraphs()"
        class="button"
        type="button"
      >
        Try Again
      </button>
    </div>
    <div
      v-else-if="graphs.status === 'loaded' && graphs.data.pageCount === 0"
      class="placeholder-container"
    >
      <p>You don't have any graphs yet. Click the button below to add a graph.</p>
      <RouterLink to="/graphs/add" class="button">Add Graph</RouterLink>
    </div>
    <div v-else class="graphs-container">
      <ul>
        <li v-for="graph in graphs.data.data" :key="graph.id">
          <GraphCard :graph="graph" />
        </li>
      </ul>
      <DataPager @previous="handlePreviousPage" @next="handleNextPage" :page="graphs.data" />
    </div>
  </Transition>
</template>

<style scoped>
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

  .heading-container {
    display: flex;
    gap: 1rem;
    align-items: center;
    margin-bottom: 1rem;

    & .button {
      background-color: var(--color-background-mute);
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
      border-width: 2px;
      border-style: outset;
      border-color: var(--color-background);
      border-image: initial;
      color: var(--color-text);
      cursor: pointer;
      font-size: 0.75rem;
    }

    & .button:active {
      border-style: inset;
    }

    @media (hover: hover) {
      & .button:hover {
        color: var(--orange);
      }
    }
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
      border-color: var(--color-background);
      border-image: initial;
      color: var(--color-text);
      cursor: pointer;
    }

    & .button:active {
      border-style: inset;
    }

    @media (hover: hover) {
      & .button:hover {
        color: var(--orange);
      }
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

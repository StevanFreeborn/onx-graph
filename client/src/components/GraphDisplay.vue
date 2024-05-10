<script setup lang="ts">
  import GraphMonitor from '@/components/GraphMonitor.vue';
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import { useGraphsService } from '@/composables/useGraphsService';
  import { useUserStore } from '@/stores/userStore';
  import { GraphStatus, type Graph } from '@/types';
  import { onMounted, ref } from 'vue';
  import GraphActionsMenu from './GraphActionsMenu.vue';
  import GraphHeading from './GraphHeading.vue';
  import OnxGraph from './OnxGraph.vue';

  const props = defineProps<{
    graphId: string;
  }>();

  type GraphData =
    | { status: 'loading' }
    | { status: 'built'; data: Graph }
    | { status: 'building'; data: Graph }
    | { status: 'not-built'; data: Graph }
    | { status: 'error' };

  const graphData = ref<GraphData>({ status: 'loading' });

  const userStore = useUserStore();
  const graphsService = useGraphsService(userStore);

  async function getGraph() {
    if (graphData.value.status === 'error') {
      graphData.value = { status: 'loading' };
    }

    const graphResult = await graphsService.getGraph(props.graphId);

    if (graphResult.err) {
      for (const error of graphResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      graphData.value = { status: 'error' };
      return;
    }

    if (graphResult.val.status === GraphStatus.Built) {
      graphData.value = { status: 'built', data: graphResult.val };
      return;
    }

    if (graphResult.val.status === GraphStatus.Building) {
      graphData.value = { status: 'building', data: graphResult.val };
      return;
    }

    graphData.value = { status: 'not-built', data: graphResult.val };
  }

  onMounted(async () => {
    await getGraph();
  });

  async function handleGraphProcessed() {
    await getGraph();
  }
</script>

<template>
  <Transition mode="out-in" class="graph-container">
    <div v-if="graphData.status === 'loading'">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphData.status === 'error'">
      <div>
        <p>There was an error loading the graph.</p>
        <button @click="() => getGraph()" type="button" class="button">Try Again</button>
      </div>
    </div>
    <div v-else-if="graphData.status === 'building'">
      <GraphHeading
        :id="graphData.data.id"
        :name="graphData.data.name"
        :status="graphData.data.status"
      />
      <GraphMonitor :graph-id="graphData.data.id" @graph-processed="handleGraphProcessed" />
    </div>
    <div v-else-if="graphData.status === 'not-built'">
      <GraphHeading
        :id="graphData.data.id"
        :name="graphData.data.name"
        :status="graphData.data.status"
      />
      <GraphActionsMenu />
      <div>
        <p>The graph has not been built yet.</p>
        <!-- TODO: Implement request to build graph again -->
        <button type="button" class="button">Build Graph</button>
      </div>
    </div>
    <div v-else :data-testid="`graph-${graphData.data.id}`">
      <GraphHeading
        :id="graphData.data.id"
        :name="graphData.data.name"
        :status="graphData.data.status"
      />
      <GraphActionsMenu />
      <OnxGraph :graph="graphData.data" />
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

  .graph-container,
  .graph-container > div:not(.heading-container) {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;

    & .button {
      background-color: var(--color-background-mute);
      padding: 0.25rem 0.5rem;
      border-radius: 0.25rem;
      border-width: 2px;
      border-style: outset;
      border-color: buttonborder;
      border-image: initial;
      color: var(--color-text);
      cursor: pointer;
      border-color: var(--color-background);
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
</style>

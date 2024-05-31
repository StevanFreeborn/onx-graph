<script setup lang="ts">
  import GraphMonitor from '@/components/GraphMonitor.vue';
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import { useGraphsService } from '@/composables/useGraphsService';
  import { GraphNotFoundError } from '@/services/graphsService';
  import { useUserStore } from '@/stores/userStore';
  import { GraphStatus, type Graph, type GraphLayout } from '@/types';
  import { computed, onMounted, ref } from 'vue';
  import GraphActionsMenu from './GraphActionsMenu.vue';
  import GraphFilter from './GraphFilter.vue';
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
    | { status: 'error' }
    | { status: 'not-found' };

  const graphData = ref<GraphData>({ status: 'loading' });
  const nodeFilters = ref<number[]>([]);
  const edgeFilters = ref<{ nodeId: number; fieldId: number }[]>([]);

  const graph = computed(() => {
    if (graphData.value.status !== 'built') {
      return null;
    }

    const nodes = graphData.value.data.nodes.filter(
      node => nodeFilters.value.includes(node.id) === false
    );

    const edgesMap = { ...graphData.value.data.edgesMap };

    for (const edgeFilter of edgeFilters.value) {
      const edges = edgesMap[edgeFilter.nodeId];

      if (edges === undefined) {
        continue;
      }

      edgesMap[edgeFilter.nodeId] = edges.filter(edge => edge.id !== edgeFilter.fieldId);
    }

    return {
      ...graphData.value.data,
      nodes,
      edgesMap,
    };
  });

  const userStore = useUserStore();
  const graphsService = useGraphsService(userStore);

  async function getGraph() {
    if (graphData.value.status === 'error') {
      graphData.value = { status: 'loading' };
    }

    const graphResult = await graphsService.getGraph(props.graphId);

    if (graphResult.err && graphResult.val.some(error => error instanceof GraphNotFoundError)) {
      graphData.value = { status: 'not-found' };
      return;
    }

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

  async function handleNameUpdate(name: string) {
    const status = graphData.value.status;

    if (status === 'error' || status === 'not-found' || status === 'loading') {
      return;
    }

    if (name === graphData.value.data.name) {
      return;
    }

    const updatedGraph = { ...graphData.value.data, name };
    const updateResult = await graphsService.updateGraph(updatedGraph);

    if (updateResult.err) {
      for (const error of updateResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      alert(updateResult.val.reduce((acc, error) => `${error.message}\n${acc}`, ''));

      return;
    }

    graphData.value = { status, data: updatedGraph };
  }

  async function updateLayout(layout: GraphLayout) {
    const status = graphData.value.status;

    if (status === 'error' || status === 'not-found' || status === 'loading') {
      return;
    }

    const updatedGraph = { ...graphData.value.data, layout };

    const updateResult = await graphsService.updateGraph(updatedGraph);

    if (updateResult.err) {
      for (const error of updateResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      alert(updateResult.val.reduce((acc, error) => `${error.message}\n${acc}`, ''));

      return;
    }

    graphData.value = { status, data: updatedGraph };
  }

  function createLayoutUpdateHandler(updateFn: (layout: GraphLayout) => Promise<void>) {
    let isWaiting = false;
    let pendingValue: GraphLayout | null = null;
    const delay = 1000;

    async function processUpdate() {
      if (pendingValue === null) {
        isWaiting = false;
        return;
      }

      await updateFn(pendingValue);
      pendingValue = null;
      setTimeout(processUpdate, delay);
    }

    return async function (layout: GraphLayout) {
      if (isWaiting) {
        pendingValue = layout;
        return;
      }

      await updateFn(layout);
      isWaiting = true;
      setTimeout(processUpdate, delay);
    };
  }

  const handleLayoutUpdate = createLayoutUpdateHandler(updateLayout);

  function handleNodeFilter(nodeId: number, show: boolean) {
    if (show) {
      nodeFilters.value = nodeFilters.value.filter(id => id !== nodeId);
      return;
    }

    nodeFilters.value.push(nodeId);
  }

  function handleEdgeFilter(nodeId: number, fieldId: number, show: boolean) {
    if (show) {
      edgeFilters.value = edgeFilters.value.filter(
        edge => edge.nodeId !== nodeId || edge.fieldId !== fieldId
      );
      return;
    }

    edgeFilters.value.push({ nodeId, fieldId });
  }

  function clearAllFilters() {
    nodeFilters.value = [];
    edgeFilters.value = [];
  }

  async function handleRefreshGraph() {
    const status = graphData.value.status;

    if (status === 'not-found' || status === 'loading') {
      return;
    }

    const refreshResult = await graphsService.refreshGraph(props.graphId);

    if (refreshResult.err) {
      for (const error of refreshResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      alert(refreshResult.val.reduce((acc, error) => `${error.message}\n${acc}`, ''));

      return;
    }

    graphData.value.status = 'building';
    clearAllFilters();
  }
</script>

<template>
  <Transition mode="out-in" class="graph-container">
    <div v-if="graphData.status === 'loading'">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphData.status === 'not-found'">
      <p>Hmm...doesn't look like that graph exists.</p>
    </div>
    <div v-else-if="graphData.status === 'error'">
      <div>
        <p>There was an error loading the graph.</p>
        <button @click="() => getGraph()" type="button" class="button">Try Again</button>
      </div>
    </div>
    <div v-else-if="graphData.status === 'building'">
      <div class="heading-filter-container">
        <GraphHeading
          :id="graphData.data.id"
          :name="graphData.data.name"
          :status="graphData.data.status"
          @update-name="handleNameUpdate"
        />
      </div>
      <GraphMonitor :graph-id="graphData.data.id" @graph-processed="handleGraphProcessed" />
    </div>
    <div v-else-if="graphData.status === 'not-built'">
      <div class="heading-filter-container">
        <GraphHeading
          :id="graphData.data.id"
          :name="graphData.data.name"
          :status="graphData.data.status"
          @update-name="handleNameUpdate"
        />
      </div>
      <GraphActionsMenu :graph-id="graphData.data.id" @refresh="handleRefreshGraph" />
      <div>
        <p>The graph has not been built yet.</p>
        <button type="button" class="button" @click="handleRefreshGraph">Build Graph</button>
      </div>
    </div>
    <div v-else-if="graph !== null" :data-testid="`graph-${graphData.data.id}`">
      <div class="heading-filter-container">
        <GraphHeading
          :id="graphData.data.id"
          :name="graphData.data.name"
          :status="graphData.data.status"
          @update-name="handleNameUpdate"
        />
        <GraphFilter
          :nodes="graphData.data.nodes"
          :edgesMap="graphData.data.edgesMap"
          @filter:node="handleNodeFilter"
          @filter:edge="handleEdgeFilter"
        />
      </div>
      <GraphActionsMenu :graph-id="graphData.data.id" @refresh="handleRefreshGraph" />
      <OnxGraph :graph="graph" @update:layout="handleLayoutUpdate" />
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
  .graph-container > div:not(.heading-filter-container) {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;

    & .heading-filter-container {
      z-index: 1;
      position: absolute;
      top: 0.5rem;
      left: 0.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

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

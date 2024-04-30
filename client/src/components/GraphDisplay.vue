<script setup lang="ts">
  import GraphMonitor from '@/components/GraphMonitor.vue';
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import { useGraphsService } from '@/composables/useGraphsService';
  import { useUserStore } from '@/stores/userStore';
  import { GraphStatus, type Graph } from '@/types';
  import { onMounted, ref } from 'vue';
  import GraphHeading from './GraphHeading.vue';

  // Overall Idea:
  // 1.  When the component is mounted we need to fetch the graph.
  // 2.  If the graph is successfully fetched, we need to check it's status.
  // 3.  If the graph's status is built, then we can display the graph.
  // 4.  If the graph's status is not build, then we should display a UI
  //     that indicates that the graph has not been built and provide a
  //     way for the user to initiate the build process.
  // 5.  When the user initiates the build process, we should make a request
  //     to the server to build the graph.
  // 6.  We should establish a websocket connection to listen for updates
  //     on the graph build process and update the UI accordingly.
  // 7.  When we receive a message from the websocket that the graph has been
  //     built, we should make a request to fetch the graph and update the UI
  //     to display the graph.
  // 8.  If the graph build fails, we should display an error message to the user.
  // 9.  If the user navigates away from the page, we should close the websocket
  //     connection.
  // 10. If the user navigates back to the page, we should re-establish the
  //     websocket connection and update the UI accordingly.
  // 11. If the graph status is building we should establish a websocket connection
  //     to listen for updates on the graph build process and update the UI accordingly.

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
</script>

<template>
  <Transition mode="out-in" class="graph-container">
    <div v-if="graphData.status === 'loading'">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphData.status === 'error'">
      <div>
        <p>There was an error loading the graph.</p>
        <button @click="() => getGraph()" type="button">Try Again</button>
      </div>
    </div>
    <div v-else-if="graphData.status === 'building'">
      <GraphHeading :name="graphData.data.name" />
      <GraphMonitor :graph-id="graphData.data.id" />
    </div>
    <div v-else-if="graphData.status === 'not-built'">
      <GraphHeading :name="graphData.data.name" />
      <div>
        <p>The graph has not been built yet.</p>
        <button type="button">Build Graph</button>
      </div>
    </div>
    <div v-else :data-testid="`graph-${graphData.data.id}`">
      <GraphHeading :name="graphData.data.name" />
      <pre>{{ graphData.data }}</pre>
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
  .graph-container > div:not(:first-child) {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;
  }
</style>

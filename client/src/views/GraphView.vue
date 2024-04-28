<script setup lang="ts">
  import SpinningLoader from '@/components/SpinningLoader.vue';
  import type { Graph } from '@/types';
  import { onMounted, ref } from 'vue';
  import { useRoute } from 'vue-router';

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

  type GraphData = { status: 'loading' } | { status: 'success'; data: Graph } | { status: 'error' };

  const graphData = ref<GraphData>({ status: 'loading' });

  const route = useRoute();

  async function getGraph() {
    if (graphData.value.status === 'error') {
      graphData.value = { status: 'loading' };
    }

    await new Promise(resolve => setTimeout(resolve, 1000));

    graphData.value = { status: 'success', data: { id: '1', name: 'Graph 1' } };
  }

  onMounted(async () => {
    await getGraph();
  });
</script>

<template>
  <div>
    <h2>{{ route.params.id }}</h2>
  </div>
  <Transition mode="out-in">
    <div v-if="graphData.status === 'loading'">
      <SpinningLoader height="3rem" width="3rem" />
    </div>
    <div v-else-if="graphData.status === 'error'">
      <div>Error</div>
    </div>
    <div v-else>
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
</style>

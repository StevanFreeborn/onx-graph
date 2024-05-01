<script setup lang="ts">
  import { GraphHubEvents, useGraphHub } from '@/composables/useGraphHub';
  import { onMounted, onUnmounted, ref } from 'vue';
  import GraphMonitorLoader from './GraphMonitorLoader.vue';

  const props = defineProps<{
    graphId: string;
  }>();

  const emits = defineEmits<{
    graphProcessed: [];
  }>();

  const message = ref<string>('Connecting to graph for updates...');
  const outcome = ref<'success' | 'error' | 'pending'>('pending');

  const connection = useGraphHub();

  connection.on(GraphHubEvents.ReceiveUpdate, data => {
    message.value = data;
  });

  connection.on(GraphHubEvents.GraphBuilt, () => {
    message.value = 'Graph built successfully!';
    outcome.value = 'success';
    emits('graphProcessed');
  });

  connection.on(GraphHubEvents.GraphError, () => {
    message.value = 'Error building graph!';
    outcome.value = 'error';
    emits('graphProcessed');
  });

  onMounted(async () => {
    try {
      await connection.start();
      await connection.invoke(GraphHubEvents.JoinGraph, props.graphId);
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error(err);
    }
  });

  onUnmounted(async () => {
    try {
      await connection.stop();
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error(err);
    }
  });
</script>

<template>
  <div>
    <GraphMonitorLoader :msg="message" :status="outcome" />
  </div>
</template>

<style scoped></style>

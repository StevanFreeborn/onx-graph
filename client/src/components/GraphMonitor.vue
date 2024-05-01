<script setup lang="ts">
  import { GraphHubEvents, useGraphHub } from '@/composables/useGraphHub';
  import { onMounted, onUnmounted, ref } from 'vue';
  import SpinningLoader from './SpinningLoader.vue';

  const props = defineProps<{
    graphId: string;
  }>();

  const emits = defineEmits<{
    graphProcessed: [];
  }>();

  const message = ref<string>('Connecting to graph for updates...');

  let connection = useGraphHub();

  connection.on(GraphHubEvents.ReceiveUpdate, data => {
    message.value = data;
  });

  connection.on(GraphHubEvents.GraphBuilt, () => {
    message.value = 'Graph built successfully!';
    emits('graphProcessed');
  });

  connection.on(GraphHubEvents.GraphError, () => {
    message.value = 'Error building graph!';
    emits('graphProcessed');
  });

  onMounted(async () => {
    try {
      await connection.start();
      await connection.invoke('JoinGraph', props.graphId);
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
    <SpinningLoader width="3rem" height="3rem" :msg="message" />
  </div>
</template>

<style scoped></style>

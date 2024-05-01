<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService.js';
  import { useUserStore } from '@/stores/userStore.js';
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { onMounted, onUnmounted, ref } from 'vue';
  import SpinningLoader from './SpinningLoader.vue';

  const props = defineProps<{
    graphId: string;
  }>();

  const message = ref<string>('Connecting to graph for updates...');
  const userStore = useUserStore();
  const authService = useAuthService(userStore);

  let connection = new HubConnectionBuilder()
    .withUrl('https://localhost:3002/graphs/hub', {
      accessTokenFactory: async () => {
        var token = userStore.user?.token ?? '';
        var expiresAt = userStore.user?.expiresAtInSeconds;
        var expiresAtInMs = expiresAt ? expiresAt * 1000 : 0;
        var now = new Date().getTime();

        if (expiresAtInMs > now) {
          return token;
        }

        const refreshResult = await authService.refreshToken();

        if (refreshResult.err) {
          for (const error of refreshResult.val) {
            // eslint-disable-next-line no-console
            console.error(error);
          }

          return token;
        }

        userStore.logUserIn(refreshResult.val.accessToken);
        return refreshResult.val.accessToken;
      },
    })
    .withAutomaticReconnect()
    .build();

  connection.on('ReceiveUpdate', data => {
    message.value = data;
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

<script setup lang="ts">
  import AccountVerifier from '@/components/AccountVerifier.vue';
  import { useRoute } from 'vue-router';

  const route = useRoute();
  const query = route.query;
  const token = query['t'];
</script>

<template>
  <main>
    <div class="container">
      <h2>Account Verification</h2>
      <Suspense>
        <AccountVerifier :token="token" />
      </Suspense>
    </div>
  </main>
</template>

<style scoped>
  main {
    display: flex;
    justify-content: center;
  }

  .container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 100%;
    gap: 2rem;
  }

  @media (min-width: 1024px) {
    main {
      flex: 1;
      width: 100%;
    }
  }
</style>

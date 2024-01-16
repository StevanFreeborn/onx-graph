<script setup lang="ts">
  import LoginForm from '@/components/LoginForm.vue';
</script>

<template>
  <main>
    <LoginForm />
  </main>
</template>

<style scoped>
  main {
    display: flex;
    justify-content: center;
    width: 100%;
  }

  @media (min-width: 1024px) {
    main {
      flex: 1;
    }
  }
</style>

<script setup lang="ts">
  import RegisterForm from '@/components/RegisterForm.vue';
</script>

<template>
  <main>
    <RegisterForm />
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

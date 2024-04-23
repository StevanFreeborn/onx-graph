<script setup lang="ts">
  import AddGraphForm from '@/components/AddGraphForm.vue';
</script>

<template>
  <h2>Graphs</h2>
  <div class="container">
    <AddGraphForm />
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    height: 100%;
  }
</style>

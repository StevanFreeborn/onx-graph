<script setup lang="ts">
  const props = defineProps<{
    name: string;
  }>();
</script>

<template>
  <div class="heading-container">
    <h2>{{ props.name }}</h2>
  </div>
</template>

<style scoped>
  .heading-container {
    position: absolute;
    top: 0;
    left: 0;
    padding: 1rem;
  }
</style>

<script setup lang="ts">
  const props = defineProps<{
    msg?: string;
    height: string;
    width: string;
    hideMsg?: boolean;
  }>();
</script>

<template>
  <div class="container">
    <div class="loader">
      <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 512 512"
        fill="currentColor"
        aria-labelledby="title"
      >
        <title id="title" lang="en">Spinning Loader</title>
        <path
          d="M222.7 32.1c5 16.9-4.6 34.8-21.5 39.8C121.8 95.6 64 169.1 64 256c0 106 86 192 192 192s192-86 192-192c0-86.9-57.8-160.4-137.1-184.1c-16.9-5-26.6-22.9-21.5-39.8s22.9-26.6 39.8-21.5C434.9 42.1 512 140 512 256c0 141.4-114.6 256-256 256S0 397.4 0 256C0 140 77.1 42.1 182.9 10.6c16.9-5 34.8 4.6 39.8 21.5z"
        />
      </svg>
    </div>
    <div v-if="hideMsg !== true">{{ msg ?? 'Loading...' }}</div>
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;

    .loader {
      animation: spin 2s linear infinite;
      height: v-bind('props.height');
      width: v-bind('props.width');
      display: flex;
      justify-content: center;
      align-items: center;
    }
  }

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
</style>

<script setup lang="ts">
  import type { Page } from '@/types';
  import { computed } from 'vue';

  const props = defineProps<{
    page: Page;
  }>();

  defineEmits<{
    previous: [];
    next: [];
  }>();

  const hasPreviousPage = computed(() => props.page.pageNumber > 1);
  const hasNextPage = computed(() => props.page.pageNumber < props.page.totalPages);
</script>

<template>
  <div class="data-pager">
    <button :disabled="hasPreviousPage === false" @click="$emit('previous')" type="button">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
        <path
          d="M9.4 233.4c-12.5 12.5-12.5 32.8 0 45.3l160 160c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L109.2 288 416 288c17.7 0 32-14.3 32-32s-14.3-32-32-32l-306.7 0L214.6 118.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0l-160 160z"
        />
      </svg>
      <span class="sr-only">Previous Page</span>
    </button>
    <span>{{ page.pageNumber }} of {{ page.totalPages }}</span>
    <button :disabled="hasNextPage === false" @click="$emit('next')" type="button">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
        <path
          d="M438.6 278.6c12.5-12.5 12.5-32.8 0-45.3l-160-160c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L338.8 224 32 224c-17.7 0-32 14.3-32 32s14.3 32 32 32l306.7 0L233.4 393.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0l160-160z"
        />
      </svg>
      <span class="sr-only">Next Page</span>
    </button>
  </div>
</template>

<style scoped>
  .data-pager {
    display: flex;
    justify-content: center;
    gap: 1rem;

    & button {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 1.75rem;
      height: 1.75rem;
      color: var(--color-text);
      background-color: var(--color-background-mute);
      border-radius: 50%;
      cursor: pointer;
    }

    & button:disabled {
      cursor: not-allowed;
      opacity: 0.5;
    }

    @media (hover: hover) {
      & button:not(:disabled):hover {
        color: var(--orange);
      }
    }
  }
</style>

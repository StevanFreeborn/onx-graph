<script setup lang="ts">
  import { useMounted } from '@/composables/useMounted';
  import { computed, onMounted, onUnmounted } from 'vue';

  const props = defineProps<{
    show: boolean;
    confirmButtonText: string;
    cancelButtonText: string;
  }>();

  const emit = defineEmits<{
    confirm: [];
    cancel: [];
  }>();

  const overlayClasses = computed(() => ({
    overlay: true,
    visible: props.show,
  }));

  const mounted = useMounted();

  function handleClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;

    if (props.show && target.closest('.overlay')) {
      emit('cancel');
    }
  }

  onMounted(() => {
    document.addEventListener('click', handleClickOutside);
  });

  onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside);
  });
</script>

<template>
  <div :class="overlayClasses">
    <div class="confirm">
      <slot></slot>
      <div class="buttons-container">
        <button type="button" :disabled="mounted === false" @click="$emit('confirm')">
          {{ confirmButtonText }}
        </button>
        <button type="button" :disabled="mounted === false" @click="$emit('cancel')">
          {{ cancelButtonText }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .overlay {
    z-index: 1000;
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 100%;
    display: none;
    justify-content: center;
    align-items: center;
    background-color: rgba(0, 0, 0, 0.2);
    border-radius: 0.5rem;

    &.visible {
      display: flex;
    }

    & .confirm {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      background-color: var(--color-background);
      border-radius: 0.5rem;
      padding: 1rem;
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);

      & .buttons-container {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 1rem;

        button {
          display: flex;
          align-items: center;
          justify-content: center;
          border-radius: 0.25rem;
          padding: 0.25rem 0.5rem;
          background-color: inherit;
          color: var(--color-text);
          cursor: pointer;
          border-color: var(--color-background);
        }

        @media (hover: hover) {
          button:hover {
            color: var(--orange);
          }
        }
      }
    }
  }
</style>

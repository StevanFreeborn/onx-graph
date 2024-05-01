<script setup lang="ts">
  import { computed } from 'vue';

  const props = defineProps<{
    msg: string;
    status: 'success' | 'error' | 'pending';
  }>();

  const checkmarkClasses = computed(() => {
    return {
      checkmark: true,
      visible: props.status === 'success',
    };
  });

  const xClasses = computed(() => {
    return {
      x: true,
      visible: props.status === 'error',
    };
  });
</script>

<template>
  <div class="container">
    <div :class="checkmarkClasses"></div>
    <div :class="xClasses"></div>
    <div class="circle">
      <div class="checkmark-icon"></div>
      <div class="x-icon"></div>
    </div>
    <div>{{ msg }}</div>
  </div>
</template>

<style scoped>
  .container {
    --circle-size: 3.5rem;
    --success-color: green;
    --error-color: red;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    justify-content: center;
    align-items: center;
    height: 100%;
    width: 100%;

    & .checkmark,
    & .x {
      display: none;
    }

    & .circle {
      display: inline-block;
      height: var(--circle-size);
      width: var(--circle-size);
      border: 2px solid rgba(255, 255, 255, 0.2);
      border-radius: 50%;
      border-left-color: var(--orange);
      position: relative;
      animation: rotate 1.2s linear infinite;

      & .checkmark-icon,
      & .x-icon {
        display: none;
      }

      .checkmark-icon::after {
        position: absolute;
        content: '';
        height: 1.75rem;
        width: 1rem;
        border-top: 4px solid var(--success-color);
        border-right: 4px solid var(--success-color);
        left: 0.75rem;
        top: 50%;
        transform: scaleX(-1) rotate(135deg);
        transform-origin: left top;
        animation: checkmark-icon 0.8s ease;
      }

      .x-icon::after,
      .x-icon::before {
        position: absolute;
        content: '';
        height: 2rem;
        border-top: 4px solid var(--error-color);
        border-right: 4px solid var(--error-color);
        top: 50%;
        left: 50%;
        animation: x-icon 0.8s ease;
      }

      .x-icon::after {
        transform: translate(-50%, -50%) rotate(45deg);
      }

      .x-icon::before {
        transform: translate(-50%, -50%) rotate(-45deg);
      }
    }

    & .checkmark.visible ~ .circle {
      animation: none;
      border-color: var(--success-color);
      transition: border 0.5s ease-out;
    }

    & .x.visible ~ .circle {
      animation: none;
      border-color: #d9534f;
      transition: border 0.5s ease-out;
    }

    & .checkmark.visible ~ .circle .checkmark-icon,
    & .x.visible ~ .circle .x-icon {
      display: block;
    }
  }

  @keyframes rotate {
    100% {
      transform: rotate(360deg);
    }
  }

  @keyframes checkmark-icon {
    0% {
      height: 0;
      width: 0;
      opacity: 1;
    }
    20% {
      height: 0;
      width: 1rem;
      opacity: 1;
    }
    40% {
      height: 1.75rem;
      width: 1rem;
      opacity: 1;
    }
    100% {
      height: 1.75rem;
      width: 1rem;
      opacity: 1;
    }
  }

  @keyframes x-icon {
    0% {
      height: 0;
      opacity: 1;
    }
    100% {
      height: 2rem;
      opacity: 1;
    }
  }
</style>

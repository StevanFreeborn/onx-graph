<script setup lang="ts">
  import { computed, onMounted, onUnmounted, ref } from 'vue';

  const menuVisible = ref(false);
  const menuClasses = computed(() => ({
    menu: true,
    visible: menuVisible.value,
  }));

  onMounted(() => {
    document.addEventListener('click', handleClickOutside);
  });

  onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside);
  });

  function handleClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;

    if (target.closest('.menu-container') === null) {
      menuVisible.value = false;
    }
  }

  function toggleMenu() {
    menuVisible.value = !menuVisible.value;
  }
</script>

<template>
  <div class="menu-container">
    <button type="button" @click="toggleMenu">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 512" fill="currentColor">
        <path
          d="M64 360a56 56 0 1 0 0 112 56 56 0 1 0 0-112zm0-160a56 56 0 1 0 0 112 56 56 0 1 0 0-112zM120 96A56 56 0 1 0 8 96a56 56 0 1 0 112 0z"
        />
      </svg>
    </button>
    <div class="menu" :class="menuClasses">
      <ul>
        <li>Delete Graph</li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
  .menu-container {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    z-index: 2;
    max-width: max-content;

    & button {
      position: absolute;
      top: 0;
      right: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: var(--color-background);
      border: none;
      color: var(--color-text);
      cursor: pointer;
      height: 1.5rem;
      width: 1.5rem;
      padding: 0.25rem;
      border-radius: 0.25rem;
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);

      & svg {
        height: 0.75rem;
        width: 0.75rem;
      }
    }

    @media (hover: hover) {
      & button:hover {
        color: var(--orange);
      }
    }

    & .menu {
      display: none;
      position: absolute;
      top: 0;
      right: 1.75rem;
      background-color: var(--color-background);
      border-radius: 0.25rem;
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      padding: 0.5rem;
      white-space: nowrap;
    }

    & .menu.visible {
      display: block;
    }
  }
</style>

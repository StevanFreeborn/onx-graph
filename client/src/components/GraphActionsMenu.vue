<script setup lang="ts">
  import { useMounted } from '@/composables/useMounted.js';
  import { computed, onMounted, onUnmounted, ref } from 'vue';
  import ConfirmDialog from './ConfirmDialog.vue';

  const mounted = useMounted();
  const menuVisible = ref(false);
  const showConfirmDialog = ref(false);
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

    if (target.closest('button.menu-button')) {
      return;
    }

    if (target.closest('.menu') === null) {
      menuVisible.value = false;
    }
  }

  function toggleMenu() {
    menuVisible.value = !menuVisible.value;
  }

  function handleDeleteGraph() {
    menuVisible.value = false;
    showConfirmDialog.value = true;
  }

  function handleConfirmDelete() {
    showConfirmDialog.value = false;
  }

  function handleCancelDelete() {
    showConfirmDialog.value = false;
  }
</script>

<template>
  <div class="menu-container">
    <button class="menu-button" type="button" @click="toggleMenu" :disabled="mounted === false">
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 512" fill="currentColor">
        <path
          d="M64 360a56 56 0 1 0 0 112 56 56 0 1 0 0-112zm0-160a56 56 0 1 0 0 112 56 56 0 1 0 0-112zM120 96A56 56 0 1 0 8 96a56 56 0 1 0 112 0z"
        />
      </svg>
    </button>
    <div class="menu" :class="menuClasses">
      <ul>
        <li>
          <button type="button" @click="handleDeleteGraph" :disabled="mounted === false">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
              <path
                d="M135.2 17.7L128 32H32C14.3 32 0 46.3 0 64S14.3 96 32 96H416c17.7 0 32-14.3 32-32s-14.3-32-32-32H320l-7.2-14.3C307.4 6.8 296.3 0 284.2 0H163.8c-12.1 0-23.2 6.8-28.6 17.7zM416 128H32L53.2 467c1.6 25.3 22.6 45 47.9 45H346.9c25.3 0 46.3-19.7 47.9-45L416 128z"
              />
            </svg>
            <span>Delete Graph</span>
          </button>
        </li>
      </ul>
    </div>
  </div>
  <ConfirmDialog
    :show="showConfirmDialog"
    confirmButtonText="Delete"
    cancelButtonText="Cancel"
    @confirm="handleConfirmDelete"
    @cancel="handleCancelDelete"
  >
    <img src="/you_sure_about_that.gif" alt="Are you sure about that?" class="confirm-image" />
  </ConfirmDialog>
</template>

<style scoped>
  .menu-container {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    z-index: 2;
    max-width: max-content;

    & .menu-button {
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

      & button {
        display: flex;
        align-items: center;
        padding: 0.25rem 0.5rem;
        border-radius: 0.25rem;
        gap: 0.5rem;
        font-size: 0.75rem;
        background-color: var(--color-background);
        border: none;
        color: var(--color-text);
        cursor: pointer;
        width: 100%;

        & svg {
          height: 0.75rem;
          width: 0.75rem;
        }
      }

      & button:active {
        background-color: var(--color-background-mute);
      }
    }

    & .menu.visible {
      display: block;
    }
  }

  .confirm-image {
    width: 200px;
    height: auto;
    border-radius: 0.25rem;
  }
</style>

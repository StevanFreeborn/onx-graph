<script setup lang="ts">
  import { useGraphsService } from '@/composables/useGraphsService';
  import { useMounted } from '@/composables/useMounted.js';
  import { useUserStore } from '@/stores/userStore';
  import { onMounted, onUnmounted, ref } from 'vue';
  import { useRouter } from 'vue-router';
  import ConfirmDialog from './ConfirmDialog.vue';

  const props = defineProps<{
    graphId: string;
  }>();

  const emit = defineEmits<{
    refresh: [];
  }>();

  const router = useRouter();
  const mounted = useMounted();
  const userStore = useUserStore();
  const graphService = useGraphsService(userStore);
  const menuVisible = ref(false);
  const showConfirmDialog = ref(false);

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

  function handleCancelDelete() {
    showConfirmDialog.value = false;
  }

  async function handleConfirmDelete() {
    showConfirmDialog.value = false;

    const deleteResult = await graphService.deleteGraph(props.graphId);

    if (deleteResult.err) {
      alert('Unable to delete graph. Please try again.');
      // eslint-disable-next-line no-console
      for (const error of deleteResult.val) {
        // eslint-disable-next-line no-console
        console.error(error);
      }

      return;
    }

    router.push('/graphs');
  }

  async function handleRefreshGraph() {
    menuVisible.value = false;
    emit('refresh');
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
      <span class="sr-only">Toggle Actions Menu</span>
    </button>
    <div v-if="menuVisible" class="menu" role="menu">
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
        <li>
          <button type="button" @click="handleRefreshGraph" :disabled="mounted === false">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
              <path
                d="M105.1 202.6c7.7-21.8 20.2-42.3 37.8-59.8c62.5-62.5 163.8-62.5 226.3 0L386.3 160H352c-17.7 0-32 14.3-32 32s14.3 32 32 32H463.5c0 0 0 0 0 0h.4c17.7 0 32-14.3 32-32V80c0-17.7-14.3-32-32-32s-32 14.3-32 32v35.2L414.4 97.6c-87.5-87.5-229.3-87.5-316.8 0C73.2 122 55.6 150.7 44.8 181.4c-5.9 16.7 2.9 34.9 19.5 40.8s34.9-2.9 40.8-19.5zM39 289.3c-5 1.5-9.8 4.2-13.7 8.2c-4 4-6.7 8.8-8.1 14c-.3 1.2-.6 2.5-.8 3.8c-.3 1.7-.4 3.4-.4 5.1V432c0 17.7 14.3 32 32 32s32-14.3 32-32V396.9l17.6 17.5 0 0c87.5 87.4 229.3 87.4 316.7 0c24.4-24.4 42.1-53.1 52.9-83.7c5.9-16.7-2.9-34.9-19.5-40.8s-34.9 2.9-40.8 19.5c-7.7 21.8-20.2 42.3-37.8 59.8c-62.5 62.5-163.8 62.5-226.3 0l-.1-.1L125.6 352H160c17.7 0 32-14.3 32-32s-14.3-32-32-32H48.4c-1.6 0-3.2 .1-4.8 .3s-3.1 .5-4.6 1z"
              />
            </svg>
            <span>Refresh Graph</span>
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
      display: block;
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
  }

  .confirm-image {
    width: 200px;
    height: auto;
    border-radius: 0.25rem;
  }
</style>

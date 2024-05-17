<script setup lang="ts">
  import { useGraphsService } from '@/composables/useGraphsService.js';
  import { useMounted } from '@/composables/useMounted.js';
  import { useUserStore } from '@/stores/userStore.js';
  import { GraphStatus } from '@/types';
  import { ref } from 'vue';

  const props = defineProps<{
    id: string;
    name: string;
    status: GraphStatus;
  }>();

  const emit = defineEmits<{
    updateName: [string];
  }>();

  const mounted = useMounted();
  const userStore = useUserStore();
  const graphService = useGraphsService(userStore);

  const nameDisabled = ref<boolean>(true);
  const fakeApiKey = '*'.repeat(32);
  const apiKey = ref<string>(fakeApiKey);
  const showApiKey = ref<boolean>(false);

  async function toggleApiKeyVisibility() {
    showApiKey.value = !showApiKey.value;

    const isFakeKey = apiKey.value === fakeApiKey;

    if (isFakeKey) {
      const keyResponse = await graphService.getGraphKey(props.id);

      if (keyResponse.err) {
        for (const error of keyResponse.val) {
          // eslint-disable-next-line no-console
          console.error(error);
        }

        apiKey.value = 'Hmmm...Something went wrong.';
        return;
      }

      apiKey.value = keyResponse.val.key;
      return;
    }

    apiKey.value = fakeApiKey;
  }

  async function handleNameFocus() {
    nameDisabled.value = false;
  }

  async function handleNameBlur(event: FocusEvent) {
    nameDisabled.value = true;
    emit('updateName', (event.target as HTMLInputElement).value);
  }
</script>

<template>
  <div class="heading-container">
    <div class="heading-row">
      <div class="heading-wrapper">
        <label for="name" class="sr-only">Graph Name</label>
        <input
          id="name"
          name="name"
          :value="props.name"
          :title="props.name"
          @focus="handleNameFocus"
          @blur="handleNameBlur"
          :readonly="nameDisabled === true"
        />
      </div>
      <div class="status-icon-container">
        <span v-if="props.status === GraphStatus.Built" class="status-icon built" title="Built">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512" fill="currentColor">
            <path
              d="M48 0C21.5 0 0 21.5 0 48V464c0 26.5 21.5 48 48 48h96V432c0-26.5 21.5-48 48-48s48 21.5 48 48v80h96c15.1 0 28.5-6.9 37.3-17.8C340.4 462.2 320 417.5 320 368c0-54.7 24.9-103.5 64-135.8V48c0-26.5-21.5-48-48-48H48zM64 240c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V240zm112-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V240c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V240zM80 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V112zM272 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zM640 368a144 144 0 1 0 -288 0 144 144 0 1 0 288 0zm-76.7-43.3c6.2 6.2 6.2 16.4 0 22.6l-72 72c-6.2 6.2-16.4 6.2-22.6 0l-40-40c-6.2-6.2-6.2-16.4 0-22.6s16.4-6.2 22.6 0L480 385.4l60.7-60.7c6.2-6.2 16.4-6.2 22.6 0z"
            />
          </svg>
        </span>
        <span
          v-else-if="props.status === GraphStatus.Building"
          class="status-icon"
          title="Building"
        >
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512" fill="currentColor">
            <path
              d="M48 0C21.5 0 0 21.5 0 48V464c0 26.5 21.5 48 48 48h96V432c0-26.5 21.5-48 48-48s48 21.5 48 48v80h96c15.1 0 28.5-6.9 37.3-17.8C340.4 462.2 320 417.5 320 368c0-54.7 24.9-103.5 64-135.8V48c0-26.5-21.5-48-48-48H48zM64 240c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V240zm112-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V240c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V240zM80 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V112zM272 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zM496 512a144 144 0 1 0 0-288 144 144 0 1 0 0 288zm0-96a24 24 0 1 1 0 48 24 24 0 1 1 0-48zm0-144c8.8 0 16 7.2 16 16v80c0 8.8-7.2 16-16 16s-16-7.2-16-16V288c0-8.8 7.2-16 16-16z"
            />
          </svg>
        </span>
        <span
          v-else-if="props.status === GraphStatus.NotBuilt"
          class="status-icon not-built"
          title="Not Built"
        >
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512" fill="currentColor">
            <path
              d="M48 0C21.5 0 0 21.5 0 48V464c0 26.5 21.5 48 48 48h96V432c0-26.5 21.5-48 48-48s48 21.5 48 48v80h96c15.1 0 28.5-6.9 37.3-17.8C340.4 462.2 320 417.5 320 368c0-54.7 24.9-103.5 64-135.8V48c0-26.5-21.5-48-48-48H48zM64 240c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V240zm112-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V240c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V240zM80 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H80c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zm80 16c0-8.8 7.2-16 16-16h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H176c-8.8 0-16-7.2-16-16V112zM272 96h32c8.8 0 16 7.2 16 16v32c0 8.8-7.2 16-16 16H272c-8.8 0-16-7.2-16-16V112c0-8.8 7.2-16 16-16zM496 512a144 144 0 1 0 0-288 144 144 0 1 0 0 288zm59.3-180.7L518.6 368l36.7 36.7c6.2 6.2 6.2 16.4 0 22.6s-16.4 6.2-22.6 0L496 390.6l-36.7 36.7c-6.2 6.2-16.4 6.2-22.6 0s-6.2-16.4 0-22.6L473.4 368l-36.7-36.7c-6.2-6.2-6.2-16.4 0-22.6s16.4-6.2 22.6 0L496 345.4l36.7-36.7c6.2-6.2 16.4-6.2 22.6 0s6.2 16.4 0 22.6z"
            />
          </svg>
        </span>
      </div>
    </div>
    <div class="api-key-row">
      <label for="apiKey" class="sr-only">API Key</label>
      <input id="apiKey" name="apiKey" :value="apiKey" :disabled="showApiKey === false" />
      <button type="button" @click="toggleApiKeyVisibility" :disabled="mounted === false">
        <span v-if="showApiKey" class="show-api-key-icon">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 512" fill="currentColor">
            <path
              d="M38.8 5.1C28.4-3.1 13.3-1.2 5.1 9.2S-1.2 34.7 9.2 42.9l592 464c10.4 8.2 25.5 6.3 33.7-4.1s6.3-25.5-4.1-33.7L525.6 386.7c39.6-40.6 66.4-86.1 79.9-118.4c3.3-7.9 3.3-16.7 0-24.6c-14.9-35.7-46.2-87.7-93-131.1C465.5 68.8 400.8 32 320 32c-68.2 0-125 26.3-169.3 60.8L38.8 5.1zM223.1 149.5C248.6 126.2 282.7 112 320 112c79.5 0 144 64.5 144 144c0 24.9-6.3 48.3-17.4 68.7L408 294.5c8.4-19.3 10.6-41.4 4.8-63.3c-11.1-41.5-47.8-69.4-88.6-71.1c-5.8-.2-9.2 6.1-7.4 11.7c2.1 6.4 3.3 13.2 3.3 20.3c0 10.2-2.4 19.8-6.6 28.3l-90.3-70.8zM373 389.9c-16.4 6.5-34.3 10.1-53 10.1c-79.5 0-144-64.5-144-144c0-6.9 .5-13.6 1.4-20.2L83.1 161.5C60.3 191.2 44 220.8 34.5 243.7c-3.3 7.9-3.3 16.7 0 24.6c14.9 35.7 46.2 87.7 93 131.1C174.5 443.2 239.2 480 320 480c47.8 0 89.9-12.9 126.2-32.5L373 389.9z"
            />
          </svg>
          <span class="sr-only">Hide API Key</span>
        </span>
        <span v-else class="hide-api-key-icon">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 576 512" fill="currentColor">
            <path
              d="M288 32c-80.8 0-145.5 36.8-192.6 80.6C48.6 156 17.3 208 2.5 243.7c-3.3 7.9-3.3 16.7 0 24.6C17.3 304 48.6 356 95.4 399.4C142.5 443.2 207.2 480 288 480s145.5-36.8 192.6-80.6c46.8-43.5 78.1-95.4 93-131.1c3.3-7.9 3.3-16.7 0-24.6c-14.9-35.7-46.2-87.7-93-131.1C433.5 68.8 368.8 32 288 32zM144 256a144 144 0 1 1 288 0 144 144 0 1 1 -288 0zm144-64c0 35.3-28.7 64-64 64c-7.1 0-13.9-1.2-20.3-3.3c-5.5-1.8-11.9 1.6-11.7 7.4c.3 6.9 1.3 13.8 3.2 20.7c13.7 51.2 66.4 81.6 117.6 67.9s81.6-66.4 67.9-117.6c-11.1-41.5-47.8-69.4-88.6-71.1c-5.8-.2-9.2 6.1-7.4 11.7c2.1 6.4 3.3 13.2 3.3 20.3z"
            />
          </svg>
          <span class="sr-only">Show API Key</span>
        </span>
      </button>
    </div>
  </div>
</template>

<style scoped>
  .heading-container {
    z-index: 1;
    position: absolute;
    top: 0.5rem;
    left: 0.5rem;
    padding: 0.75rem 1rem 0.75rem 1rem;
    background-color: var(--color-background);
    border-radius: 0.5rem;
    box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
    overflow: hidden;
    max-width: 250px;
    display: flex;
    flex-direction: column;
    gap: 0.25rem;

    & .heading-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;

      & .heading-wrapper {
        input {
          font-size: 1.25rem;
          white-space: nowrap;
          overflow: hidden;
          text-overflow: ellipsis;
          width: 100%;
          padding: 0.25rem;
          border-radius: 0.25rem;
          background-color: var(--color-background-mute);
          color: var(--color-text);
          border: none;
        }

        & input:read-only {
          background-color: var(--color-background);
        }
      }

      .status-icon-container {
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 1;
        color: var(--orange);

        .status-icon {
          display: flex;
          justify-content: center;
          align-items: center;
          height: 1.5rem;
          width: 1.5rem;

          svg {
            height: 1.5rem;
            width: 1.5rem;
          }
        }

        .built {
          color: green;
        }

        .not-built {
          color: red;
        }
      }
    }

    & .api-key-row {
      position: relative;

      & input {
        width: 100%;
        height: 1.5rem;
        padding: 0.25rem;
        border-radius: 0.25rem;
        border: transparent;
        background-color: var(--color-background-mute);
        color: var(--color-text);
        padding-right: 1.5rem;
      }

      & input:disabled {
        background-color: var(--color-background);
      }

      & button {
        position: absolute;
        right: 1px;
        top: 3.5px;
        background-color: inherit;
        color: var(--color-text);
        border: none;
        cursor: pointer;
        height: 20px;
        border-top-right-radius: 0.25rem;
        border-bottom-right-radius: 0.25rem;

        .show-api-key-icon,
        .hide-api-key-icon {
          display: flex;
          justify-content: center;
          align-items: center;
          height: 1rem;
          width: 1rem;

          svg {
            height: 1rem;
            width: 1rem;
          }
        }
      }
    }
  }
</style>

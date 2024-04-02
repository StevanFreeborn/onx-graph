<script setup lang="ts">
  import { computed, ref } from 'vue';

  const isSidebarOpen = ref(true);

  var sidebarContainerClasses = computed(() => ({
    'sidebar-container': true,
    collapsed: !isSidebarOpen.value,
  }));

  function toggleSidebar() {
    isSidebarOpen.value = !isSidebarOpen.value;
  }
</script>

<template>
  <div :class="sidebarContainerClasses">
    <button
      @click="toggleSidebar"
      type="button"
      :aria-label="isSidebarOpen ? 'collapse' : 'expand'"
    >
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
        <path
          d="M502.6 278.6c12.5-12.5 12.5-32.8 0-45.3l-128-128c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L402.7 224 32 224c-17.7 0-32 14.3-32 32s14.3 32 32 32l370.7 0-73.4 73.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0l128-128z"
        />
      </svg>
      <span class="sr-only">{{ isSidebarOpen ? 'collapse' : 'expand' }}</span>
    </button>
    <div class="hero-heading-container">
      <h1>OnxGraph</h1>
    </div>
    <div class="navigation-container">
      <nav>
        <ul>
          <li>
            <RouterLink to="/graphs">Graphs</RouterLink>
          </li>
        </ul>
      </nav>
    </div>
    <div class="user-container"></div>
  </div>
</template>

<style scoped>
  .sidebar-container {
    --button-size: 24px;
    --transition-duration: 0.5s;
    --transition-function: ease-in-out;
    --sidebar-width: 200px;
    --sidebar-x-padding: 1rem;
    --container-width: calc(var(--sidebar-width) - 2 * var(--sidebar-x-padding));
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
    width: var(--sidebar-width);
    height: 100%;
    padding: 0.25rem var(--sidebar-x-padding);
    background-color: var(--color-background-mute);
    position: relative;
    transition-property: width, padding;
    transition-duration: var(--transition-duration);
    transition-timing-function: var(--transition-function);

    & button {
      position: absolute;
      top: 3rem;
      left: calc(-1 * var(--button-size) / 2);
      display: flex;
      align-items: center;
      width: var(--button-size);
      height: var(--button-size);
      background-color: var(--color-background-mute);
      border-radius: 50%;
      border: none;
      cursor: pointer;
      transition-property: transform;
      transition-duration: var(--transition-duration);
      transition-timing-function: var(--transition-function);
    }

    & .hero-heading-container {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      width: var(--container-width);
      overflow: hidden;
      transition-property: width;
      transition-duration: var(--transition-duration);
      transition-timing-function: var(--transition-function);

      & h1 {
        font-size: 1.75rem;
        font-weight: bold;
        color: var(--orange);
        overflow: hidden;
      }
    }

    & .navigation-container {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      width: var(--container-width);
      flex: 1;
      overflow: hidden;
      transition-property: width;
      transition-duration: var(--transition-duration);
      transition-timing-function: var(--transition-function);

      & nav {
        font-size: 1.25rem;
      }
    }

    &.collapsed {
      width: calc(var(--button-size) / 2);
      padding: 0.25rem 0 0.25rem var(--sidebar-x-padding);

      & .hero-heading-container,
      & .navigation-container {
        width: 0;
      }

      & button {
        transform: rotate(180deg);
      }
    }
  }
</style>

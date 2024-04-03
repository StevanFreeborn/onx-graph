<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService';
  import { useUserStore } from '@/stores/userStore';
  import { computed, ref } from 'vue';
  import { useRouter } from 'vue-router';

  const isSidebarOpen = ref(true);

  var sidebarContainerClasses = computed(() => ({
    'sidebar-container': true,
    collapsed: !isSidebarOpen.value,
  }));

  function toggleSidebar() {
    isSidebarOpen.value = !isSidebarOpen.value;
  }

  const navLinks = [
    {
      name: 'Graphs',
      path: '/graphs',
    },
    {
      name: 'Settings',
      path: '/settings',
    },
  ];

  const router = useRouter();
  const userStore = useUserStore();
  const authService = useAuthService(userStore);

  async function handleLogout() {
    const logoutResult = await authService.logout();

    if (logoutResult.err) {
      for (const error of logoutResult.val) {
        console.error(error);
      }
      alert('An error occurred while logging out. Please try again.');
      return;
    }

    userStore.logUserOut();
    router.push({ name: 'login' });
  }
</script>

<template>
  <div :class="sidebarContainerClasses">
    <button
      class="toggle-button"
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
          <li v-for="link in navLinks" :key="link.name">
            <RouterLink :to="link.path" class="nav-link">{{ link.name }}</RouterLink>
          </li>
        </ul>
      </nav>
    </div>
    <div class="user-container">
      <div class="user-info-container">
        <div class="user-icon-container">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512" fill="currentColor">
            <path
              d="M224 256A128 128 0 1 0 224 0a128 128 0 1 0 0 256zm-45.7 48C79.8 304 0 383.8 0 482.3C0 498.7 13.3 512 29.7 512H418.3c16.4 0 29.7-13.3 29.7-29.7C448 383.8 368.2 304 269.7 304H178.3z"
            />
          </svg>
        </div>
        <div class="user-identifier" :title="userStore.user?.id">{{ userStore.user?.id }}</div>
      </div>
      <div class="logout-container">
        <button class="logout-button" type="button" @click="handleLogout">
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
            <path
              d="M377.9 105.9L500.7 228.7c7.2 7.2 11.3 17.1 11.3 27.3s-4.1 20.1-11.3 27.3L377.9 406.1c-6.4 6.4-15 9.9-24 9.9c-18.7 0-33.9-15.2-33.9-33.9l0-62.1-128 0c-17.7 0-32-14.3-32-32l0-64c0-17.7 14.3-32 32-32l128 0 0-62.1c0-18.7 15.2-33.9 33.9-33.9c9 0 17.6 3.6 24 9.9zM160 96L96 96c-17.7 0-32 14.3-32 32l0 256c0 17.7 14.3 32 32 32l64 0c17.7 0 32 14.3 32 32s-14.3 32-32 32l-64 0c-53 0-96-43-96-96L0 128C0 75 43 32 96 32l64 0c17.7 0 32 14.3 32 32s-14.3 32-32 32z"
            />
          </svg>
          <span class="sr-only">Logout</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .sidebar-container {
    --button-size: 24px;
    --transition-duration: 0.5s;
    --transition-function: ease-in-out;
    --sidebar-width: 250px;
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

    & .toggle-button {
      position: absolute;
      top: 3rem;
      left: calc(-1 * var(--button-size) / 2);
      display: flex;
      align-items: center;
      width: var(--button-size);
      height: var(--button-size);
      color: var(--color-text);
      background-color: var(--color-background-mute);
      border-radius: 50%;
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
        width: 100%;

        & ul {
          display: flex;
          flex-direction: column;
          gap: 0.5rem;
        }

        & .nav-link {
          display: block;
          width: 100%;
          color: var(--color-text);
          text-align: right;
          padding: 0.25rem 1rem;
          border-radius: 0.5rem;
        }

        & .nav-link.router-link-exact-active {
          background-color: var(--color-background-soft);
        }

        @media (hover: hover) {
          & .nav-link:hover {
            color: var(--orange);
          }
        }
      }
    }

    & .user-container {
      display: flex;
      justify-content: space-between;
      gap: 0.5rem;
      padding: 1rem;
      margin: -0.25rem calc(-1 * var(--sidebar-x-padding));
      background-color: var(--color-background-soft);

      & .user-icon-container {
        display: flex;
        flex-shrink: 0;
        align-items: center;
        justify-content: center;
        border-radius: 0.5rem;
        width: 3rem;
        height: 3rem;
        padding: 0.5rem;
        background-color: var(--color-background-mute);
      }

      & .user-info-container {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        overflow: hidden;
        flex: 1;

        & .user-identifier {
          overflow: hidden;
          text-overflow: ellipsis;
        }
      }

      & .logout-container {
        display: flex;
        align-items: center;
        flex-shrink: 0;

        & .logout-button {
          display: flex;
          align-items: center;
          justify-content: center;
          width: 2rem;
          height: 2rem;
          border-radius: 0.5rem;
          background-color: inherit;
          color: var(--color-text);
          cursor: pointer;
        }

        @media (hover: hover) {
          & .logout-button:hover {
            color: var(--orange);
          }
        }
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

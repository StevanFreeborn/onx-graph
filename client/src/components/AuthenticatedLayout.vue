<script setup lang="ts">
  import NavigationSidebar from './NavigationSidebar.vue';
</script>

<template>
  <div class="container">
    <main>
      <RouterView />
    </main>
    <aside>
      <NavigationSidebar />
    </aside>
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    height: 100%;
    width: 100%;
  }

  main {
    display: flex;
    flex-direction: column;
    flex: 1;
    padding: 1rem;
    overflow: auto;
    scrollbar-width: thin;
    scrollbar-color: var(--color-background-mute) var(--color-background);
  }

  @media screen and (max-width: 768px) {
    main {
      padding-right: 2rem;
    }
  }

  aside {
    position: relative;
  }
</style>

<script setup lang="ts">
  import { RouterLink, RouterView } from 'vue-router';
  import HeroGreeting from './HeroGreeting.vue';
</script>

<template>
  <div class="container">
    <header>
      <img alt="OnxGraph logo" class="logo" src="@/assets/logo.svg" width="125" height="125" />

      <div class="wrapper">
        <HeroGreeting />

        <nav>
          <RouterLink to="/masses/login">Login</RouterLink>
          <RouterLink to="/masses/register">Register</RouterLink>
        </nav>
      </div>
    </header>
    <RouterView />
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-height: 100%;
    width: 100%;
    gap: 2rem;
    max-width: 1280px;
    margin: 0 auto;
    padding: 2rem;
    font-weight: normal;
  }

  @media (min-width: 1024px) {
    .container {
      flex-direction: row;
      padding: 0 2rem;
    }
  }

  header {
    line-height: 1.5;
    max-height: 100vh;
  }

  .logo {
    display: block;
    margin: 0 auto 2rem;
  }

  nav {
    width: 100%;
    font-size: 1rem;
    text-align: center;
    margin-top: 2rem;
  }

  nav a.router-link-exact-active {
    color: var(--color-text);
  }

  nav a.router-link-exact-active:hover {
    background-color: transparent;
  }

  nav a {
    display: inline-block;
    padding: 0 1rem;
    border-left: 1px solid var(--color-border);
  }

  nav a:first-of-type {
    border: 0;
  }

  @media (min-width: 1024px) {
    header {
      display: flex;
      place-items: center;
      flex: 1;
    }

    .logo {
      margin: 0 2rem 0 0;
    }

    header .wrapper {
      display: flex;
      place-items: flex-start;
      flex-wrap: wrap;
    }

    nav {
      text-align: left;
      margin-left: -1rem;
      font-size: 1rem;
      padding: 1rem 0;
      margin-top: 1rem;
    }
  }
</style>

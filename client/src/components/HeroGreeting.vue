<script setup lang="ts"></script>

<template>
  <div class="greetings">
    <h1 class="orange">OnxGraph</h1>
    <h2>
      Create visualizations of your
      <a href="https://onspring.com" target="_blank" rel="noopener">Onspring</a>
      relationships
    </h2>
  </div>
</template>

<style scoped>
  h1 {
    font-weight: 500;
    font-size: 2.6rem;
    position: relative;
    top: -10px;
  }

  h2 {
    font-size: 1.2rem;
  }

  .greetings h1,
  .greetings h2 {
    text-align: center;
  }

  @media (min-width: 1024px) {
    .greetings h1,
    .greetings h2 {
      text-align: left;
    }
  }
</style>

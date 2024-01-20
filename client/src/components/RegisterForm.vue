<script setup lang="ts"></script>

<template>
  <div class="container">
    <h2>Register</h2>
    <form class="register-form">
      <div class="form-group">
        <label for="email">Email</label>
        <input type="email" id="email" />
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input type="password" id="password" />
      </div>
      <div class="form-group">
        <label for="password">Confirm Password</label>
        <input type="password" id="confirmPassword" />
      </div>
      <button class="register-button" type="submit">Register</button>
    </form>
  </div>
</template>

<style scoped>
  .container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 2rem;
    border-radius: 1rem;
    background-color: var(--color-background-soft);
    width: 100%;
    max-width: 600px;
  }

  .register-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  .form-group {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  .form-group label {
    font-weight: bold;
    font-size: 1rem;
  }

  .form-group input {
    padding: 0.5rem;
    border-radius: 0.5rem;
    border: none;
  }

  .register-button {
    padding: 0.5rem;
    border-radius: 0.5rem;
    background-color: var(--orange);
    cursor: pointer;
    border: none;
  }
</style>

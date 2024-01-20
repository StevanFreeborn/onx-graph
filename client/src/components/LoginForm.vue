<script setup lang="ts">
  import { toTitleCase } from '@/utils';
  import { reactive } from 'vue';
  import type { FormFieldState } from './types';

  type LoginFormState = Record<'email' | 'password', FormFieldState>;

  const formState = reactive<LoginFormState>({
    email: {
      value: '',
      errorMessage: '',
    },
    password: {
      value: '',
      errorMessage: '',
    },
  });

  function handleLoginFormSubmit() {
    const keys = Object.keys(formState) as (keyof LoginFormState)[];

    for (const key of keys) {
      const field = formState[key];

      if (!field.value) {
        field.errorMessage = `${toTitleCase(key)} is required.`;
        continue;
      }

      field.errorMessage = '';
    }

    if (Object.values(formState).some(field => !field.errorMessage)) {
      return;
    }

    // TODO: actually log in
  }

  function handleInputChange(e: Event) {
    const target = e.target as HTMLInputElement;
    const { id } = target;
    const error = formState[id as keyof LoginFormState].errorMessage;

    if (error) {
      formState[id as keyof LoginFormState].errorMessage = '';
    }
  }
</script>

<template>
  <div class="container">
    <h2>Login</h2>
    <form class="login-form" novalidate @submit.prevent="handleLoginFormSubmit">
      <div class="form-group">
        <label for="email">Email</label>
        <input
          type="email"
          id="email"
          name="email"
          required
          v-model="formState.email.value"
          @input="handleInputChange"
          :class="{ invalid: formState.email.errorMessage }"
        />
        <span class="error-message" v-if="formState.email.errorMessage">
          {{ formState.email.errorMessage }}
        </span>
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input
          type="password"
          id="password"
          name="password"
          required
          v-model="formState.password.value"
          @input="handleInputChange"
          :class="{ invalid: formState.email.errorMessage }"
        />
        <span class="error-message" v-if="formState.password.errorMessage">
          {{ formState.password.errorMessage }}
        </span>
      </div>
      <button class="login-button" type="submit">Login</button>
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

  .login-form {
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

  .form-group input.invalid {
    border: 0.125rem solid #bb0000;
  }

  .error-message {
    color: #bb0000;
    font-size: 0.75rem;
    font-weight: bold;
  }

  .login-button {
    padding: 0.5rem;
    border-radius: 0.5rem;
    background-color: var(--orange);
    cursor: pointer;
    border: none;
  }
</style>

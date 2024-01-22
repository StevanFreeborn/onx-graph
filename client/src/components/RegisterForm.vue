<script setup lang="ts">
  import { toTitleCase } from '@/utils';
  import isEmail from 'validator/es/lib/isEmail';
  import { reactive } from 'vue';
  import { type FormFieldState } from './types';

  type RegisterFormState = Record<'email' | 'password' | 'confirmPassword', FormFieldState>;

  const formState = reactive<RegisterFormState>({
    email: {
      value: '',
      errorMessage: '',
    },
    password: {
      value: '',
      errorMessage: '',
    },
    confirmPassword: {
      value: '',
      errorMessage: '',
    },
  });

  function handleRegisterFormSubmit() {
    const keys = Object.keys(formState) as (keyof RegisterFormState)[];

    for (const key of keys) {
      const field = formState[key];

      if (!field.value) {
        field.errorMessage = `${toTitleCase(key)} is required.`;
        continue;
      }

      if (key === 'email' && !isEmail(field.value) === false) {
        field.errorMessage = 'Enter a valid email address.';
        continue;
      }

      if (key === 'confirmPassword' && field.value !== formState.password.value) {
        field.errorMessage = 'Passwords do not match.';
        continue;
      }

      field.errorMessage = '';
    }

    if (Object.values(formState).some(field => !field.errorMessage)) {
      return;
    }

    // TODO: actually register
  }

  function handleInputChange(e: Event) {
    const target = e.target as HTMLInputElement;
    const { id } = target;
    const error = formState[id as keyof RegisterFormState].errorMessage;

    if (error) {
      formState[id as keyof RegisterFormState].errorMessage = '';
    }
  }
</script>

<template>
  <div class="container">
    <h2>Register</h2>
    <form class="register-form" novalidate @submit.prevent="handleRegisterFormSubmit">
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
          :class="{ invalid: formState.password.errorMessage }"
        />
      </div>
      <div class="form-group">
        <label for="password">Confirm Password</label>
        <input
          type="password"
          id="confirmPassword"
          name="confirmPassword"
          required
          v-model="formState.confirmPassword.value"
          @input="handleInputChange"
          :class="{ invalid: formState.confirmPassword.errorMessage }"
        />
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

  .form-group input.invalid {
    border: 0.125rem solid #bb0000;
  }

  .error-message {
    color: #bb0000;
    font-size: 0.75rem;
    font-weight: bold;
  }

  .register-button {
    padding: 0.5rem;
    border-radius: 0.5rem;
    background-color: var(--orange);
    cursor: pointer;
    border: none;
  }
</style>

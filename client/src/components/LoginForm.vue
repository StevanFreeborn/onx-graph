<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService';
  import { useUserStore } from '@/stores/userStore';
  import { toTitleCase } from '@/utils';
  import isEmail from 'validator/es/lib/isEmail';
  import { reactive } from 'vue';
  import { useRouter } from 'vue-router';
  import type { FormFieldState } from './types';

  type LoginFormState = {
    fields: Record<'email' | 'password', FormFieldState>;
    errors: string[];
  };

  const formState = reactive<LoginFormState>({
    fields: {
      email: {
        value: '',
        errorMessage: '',
      },
      password: {
        value: '',
        errorMessage: '',
      },
    },
    errors: [],
  });

  const authService = useAuthService();
  const userStore = useUserStore();
  const router = useRouter();

  async function handleLoginFormSubmit() {
    formState.errors = [];

    const keys = Object.keys(formState.fields) as (keyof LoginFormState['fields'])[];

    for (const key of keys) {
      const field = formState.fields[key];

      if (!field.value) {
        field.errorMessage = `${toTitleCase(key)} is required.`;
        continue;
      }

      if (key === 'email' && isEmail(field.value) === false) {
        field.errorMessage = 'Enter a valid email address.';
        continue;
      }

      field.errorMessage = '';
    }

    const formStateHasError = Object.values(formState.fields).some(field => field.errorMessage);

    if (formStateHasError) {
      return;
    }

    const loginResult = await authService.login(
      formState.fields.email.value,
      formState.fields.password.value
    );

    if (loginResult.err) {
      formState.errors.push(...loginResult.val.map(err => err.message));
      return;
    }

    userStore.logUserIn(loginResult.val.accessToken);
    router.push('/graphs');
  }

  function handleInputChange(e: Event) {
    const target = e.target as HTMLInputElement;
    const { id } = target;
    const fieldKey = id as keyof LoginFormState['fields'];
    const error = formState.fields[fieldKey].errorMessage;

    if (error) {
      formState.fields[fieldKey].errorMessage = '';
    }
  }
</script>

<template>
  <div class="container">
    <h2>Login</h2>
    <form
      aria-label="login form"
      class="login-form"
      novalidate
      @submit.prevent="handleLoginFormSubmit"
    >
      <div class="form-group">
        <label for="email">Email</label>
        <input
          type="email"
          id="email"
          name="email"
          required
          v-model="formState.fields.email.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.email.errorMessage }"
        />
        <span class="error-message" v-if="formState.fields.email.errorMessage">
          {{ formState.fields.email.errorMessage }}
        </span>
      </div>
      <div class="form-group">
        <label for="password">Password</label>
        <input
          type="password"
          id="password"
          name="password"
          required
          v-model="formState.fields.password.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.password.errorMessage }"
        />
        <span class="error-message" v-if="formState.fields.password.errorMessage">
          {{ formState.fields.password.errorMessage }}
        </span>
      </div>
      <button class="login-button" type="submit">Login</button>
      <ul class="form-errors">
        <li class="error-message" v-for="error in formState.errors" :key="error">{{ error }}</li>
      </ul>
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
    font-size: 1rem;
    font-weight: bold;
  }

  .login-button {
    padding: 0.5rem;
    border-radius: 0.5rem;
    background-color: var(--orange);
    cursor: pointer;
    border: none;
  }

  .form-errors {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
  }
</style>

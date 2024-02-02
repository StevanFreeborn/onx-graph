<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService';
  import { toTitleCase } from '@/utils';
  import isEmail from 'validator/es/lib/isEmail';
  import { reactive } from 'vue';
  import { useRouter } from 'vue-router';
  import { type FormFieldState } from './types';

  type RegisterFormState = {
    fields: Record<'email' | 'password' | 'confirmPassword', FormFieldState>;
    errors: string[];
  };

  const formState = reactive<RegisterFormState>({
    fields: {
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
    },
    errors: [],
  });

  const authService = useAuthService();
  const router = useRouter();

  async function handleRegisterFormSubmit() {
    formState.errors = [];

    const keys = Object.keys(formState.fields) as (keyof RegisterFormState['fields'])[];

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

      if (key === 'confirmPassword' && field.value !== formState.fields.password.value) {
        field.errorMessage = 'Passwords do not match.';
        continue;
      }

      field.errorMessage = '';
    }

    const formStateHasError = Object.values(formState.fields).some(field => field.errorMessage);

    if (formStateHasError) {
      return;
    }

    const registerResult = await authService.register(
      formState.fields.email.value,
      formState.fields.password.value
    );

    if (registerResult.err) {
      formState.errors.push(...registerResult.val.map(err => err.message));
      return;
    }

    router.push('/login');
  }

  function handleInputChange(e: Event) {
    const target = e.target as HTMLInputElement;
    const { id } = target;
    const fieldKey = id as keyof RegisterFormState['fields'];
    const error = formState.fields[fieldKey].errorMessage;

    if (error) {
      formState.fields[fieldKey].errorMessage = '';
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
          v-model="formState.fields.email.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.email.errorMessage }"
        />
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
      </div>
      <div class="form-group">
        <label for="password">Confirm Password</label>
        <input
          type="password"
          id="confirmPassword"
          name="confirmPassword"
          required
          v-model="formState.fields.confirmPassword.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.confirmPassword.errorMessage }"
        />
      </div>
      <button class="register-button" type="submit">Register</button>
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

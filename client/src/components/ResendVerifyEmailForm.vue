<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService.js';
  import { useMounted } from '@/composables/useMounted.js';
  import { useSubmitting } from '@/composables/useSubmitting.js';
  import { toTitleCase } from '@/utils/index.js';
  import { isEmail } from 'validator';
  import { reactive, ref } from 'vue';
  import type { FormFieldState } from './types.js';

  type RegisterFormState = {
    fields: Record<'email', FormFieldState>;
    errors: string[];
  };

  const formState = reactive<RegisterFormState>({
    fields: {
      email: {
        value: '',
        errorMessage: '',
      },
    },
    errors: [],
  });

  const isMounted = useMounted();
  const { isSubmitting, setIsSubmitting } = useSubmitting();
  const sendStatus = ref<'idle' | 'sending' | 'success' | 'error'>('idle');
  const SEND_STATUS_TIMEOUT = 1000;
  const sendStatusTransition = `opacity ${SEND_STATUS_TIMEOUT / 1000}s ease-out`;
  const authService = useAuthService();

  async function handleResendFormSubmit() {
    formState.errors = [];
    setIsSubmitting(true);

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

      field.errorMessage = '';
    }

    const formStateHasError = Object.values(formState.fields).some(field => field.errorMessage);

    if (formStateHasError) {
      setIsSubmitting(false);
      return;
    }

    sendStatus.value = 'sending';
    const resendResult = await authService.resendVerificationEmail(formState.fields.email.value);

    if (resendResult.err) {
      sendStatus.value = 'error';
      setTimeout(() => {
        sendStatus.value = 'idle';
      }, SEND_STATUS_TIMEOUT);
      formState.errors.push('Failed to resend email.');
      setIsSubmitting(false);
      return;
    }

    setIsSubmitting(false);
    formState.fields.email.value = '';
    sendStatus.value = 'success';
    setTimeout(() => {
      sendStatus.value = 'idle';
    }, SEND_STATUS_TIMEOUT);
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
  <form
    aria-label="resend verification email form"
    class="resend-form"
    novalidate
    @submit.prevent="handleResendFormSubmit"
  >
    <div class="form-row">
      <div class="form-group">
        <label for="email">Email</label>
        <div class="form-row">
          <input
            :disabled="isMounted === false"
            type="email"
            id="email"
            name="email"
            required
            v-model="formState.fields.email.value"
            @input="handleInputChange"
            :class="{ invalid: formState.fields.email.errorMessage }"
          />
          <button
            :disabled="isMounted === false || isSubmitting"
            class="resend-button"
            type="submit"
          >
            Resend
          </button>
          <span v-if="sendStatus === 'sending'">Sending email...</span>
          <Transition>
            <span v-if="sendStatus === 'error'">Unable to send email. Please try again.</span>
          </Transition>
          <Transition>
            <span v-if="sendStatus === 'success'">Email sent successfully.</span>
          </Transition>
        </div>
      </div>
    </div>
    <div class="form-row">
      <span class="error-message" v-if="formState.fields.email.errorMessage">
        {{ formState.fields.email.errorMessage ?? '' }}
      </span>
      <span class="error-message" v-if="formState.errors.length">
        {{ formState.errors.join(' ') ?? '' }}
      </span>
    </div>
  </form>
</template>

<style scoped>
  .resend-form {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .form-row {
    display: flex;
    gap: 0.5rem;
    align-items: center;
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

  .v-enter-active,
  .v-leave-active {
    transition: v-bind(sendStatusTransition);
  }

  .v-enter-from {
    opacity: 100;
  }

  .v-leave-to {
    opacity: 0;
  }

  @keyframes fade-out {
    0% {
      opacity: 1;
    }
    100% {
      opacity: 0;
    }
  }

  .error-message {
    color: #bb0000;
    font-size: 0.75rem;
    font-weight: bold;
  }

  .resend-button {
    padding: 0.5rem;
    border-radius: 0.5rem;
    background-color: var(--orange);
    cursor: pointer;
    border: none;
    height: fit-content;
  }
</style>

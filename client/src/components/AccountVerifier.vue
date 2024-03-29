<script setup lang="ts">
  import { useAuthService } from '@/composables/useAuthService';
  import { defineProps, ref } from 'vue';
  import { useRouter, type LocationQueryValue } from 'vue-router';
  import ResendVerifyEmailForm from './ResendVerifyEmailForm.vue';

  const props = defineProps<{
    token: LocationQueryValue | LocationQueryValue[];
  }>();

  type VerificationStatus =
    | 'Not Started'
    | 'Retrieving Token'
    | 'Verifying Token'
    | 'Verified'
    | 'Unverified';

  const verificationStatus = ref<VerificationStatus>('Not Started');
  const errorMessages = ref<string[]>([]);
  const WAIT_TIME = 1000;

  const router = useRouter();
  const authService = useAuthService();

  setTimeout(() => {
    verificationStatus.value = 'Retrieving Token';

    setTimeout(async () => {
      if (typeof props.token !== 'string') {
        verificationStatus.value = 'Unverified';
        return;
      }

      verificationStatus.value = 'Verifying Token';

      const response = await authService.verifyAccount(props.token);

      if (response.err) {
        setTimeout(() => {
          verificationStatus.value = 'Unverified';
          errorMessages.value = response.val.map(err => err.message);
        }, WAIT_TIME);
        return;
      }

      setTimeout(() => {
        verificationStatus.value = 'Verified';

        setTimeout(() => {
          router.push({ name: 'login' });
        }, WAIT_TIME);
      }, WAIT_TIME);
    }, WAIT_TIME);
  }, WAIT_TIME);

  const contactEmailAddress = import.meta.env.VITE_SENDING_EMAIL;
  const emailLink = `mailto:${contactEmailAddress}`;
</script>

<template>
  <div class="verification-container">
    <div>
      <span
        v-if="verificationStatus !== 'Verified' && verificationStatus !== 'Unverified'"
        class="icon-container spinner"
      >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
          <path
            d="M222.7 32.1c5 16.9-4.6 34.8-21.5 39.8C121.8 95.6 64 169.1 64 256c0 106 86 192 192 192s192-86 192-192c0-86.9-57.8-160.4-137.1-184.1c-16.9-5-26.6-22.9-21.5-39.8s22.9-26.6 39.8-21.5C434.9 42.1 512 140 512 256c0 141.4-114.6 256-256 256S0 397.4 0 256C0 140 77.1 42.1 182.9 10.6c16.9-5 34.8 4.6 39.8 21.5z"
          />
        </svg>
      </span>
      <span v-if="verificationStatus === 'Verified'" class="icon-container">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
          <path
            d="M256 512A256 256 0 1 0 256 0a256 256 0 1 0 0 512zM369 209L241 337c-9.4 9.4-24.6 9.4-33.9 0l-64-64c-9.4-9.4-9.4-24.6 0-33.9s24.6-9.4 33.9 0l47 47L335 175c9.4-9.4 24.6-9.4 33.9 0s9.4 24.6 0 33.9z"
          />
        </svg>
      </span>
      <span v-if="verificationStatus === 'Unverified'" class="icon-container">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" fill="currentColor">
          <path
            d="M256 512A256 256 0 1 0 256 0a256 256 0 1 0 0 512zM175 175c9.4-9.4 24.6-9.4 33.9 0l47 47 47-47c9.4-9.4 24.6-9.4 33.9 0s9.4 24.6 0 33.9l-47 47 47 47c9.4 9.4 9.4 24.6 0 33.9s-24.6 9.4-33.9 0l-47-47-47 47c-9.4 9.4-24.6 9.4-33.9 0s-9.4-24.6 0-33.9l47-47-47-47c-9.4-9.4-9.4-24.6 0-33.9z"
          />
        </svg>
      </span>
    </div>
    <div class="verification-status-container">
      <div class="statuses">
        <p v-if="verificationStatus === 'Not Started'">Starting verification process...</p>
        <p v-if="verificationStatus === 'Retrieving Token'">Retrieving Token...</p>
        <p v-if="verificationStatus === 'Verifying Token'">Verifying Token...</p>
        <p v-if="verificationStatus === 'Verified'">
          Successfully verified account! Redirecting to login...
        </p>
        <p v-if="verificationStatus === 'Unverified'">
          Uh oh! We were unable to verify your account. See below for more information:
        </p>
      </div>
      <ul v-if="errorMessages.length > 0" class="error-list">
        <li v-for="msg in errorMessages" :key="msg">
          <span class="error-message">{{ msg }}</span>
        </li>
      </ul>
      <div v-if="verificationStatus === 'Unverified'" class="resend-form-container">
        <p>
          You maybe able to resolve this issue by requesting a new verification email. You use the
          form below to do so.
        </p>
        <ResendVerifyEmailForm />
        <p>
          Please note that requesting a new verification email will invalidate the previous one. If
          you continue to experience issues, please contact us at
          <a :href="emailLink">{{ contactEmailAddress }}</a> for assistance.
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .verification-container {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 1rem;
  }

  .verification-status-container,
  .resend-form-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  .icon-container {
    display: flex;
    justify-content: center;
    align-items: center;
    width: 50px;
    height: 50px;
  }

  .spinner > svg {
    animation: spin 1s linear infinite;
  }

  .statuses {
    text-align: center;
  }

  .error-list {
    padding: 0 1rem;
    text-align: center;
  }

  .error-message {
    color: red;
  }

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
</style>

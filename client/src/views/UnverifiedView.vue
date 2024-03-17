<script setup lang="ts">
  import ResendVerifyEmailForm from '@/components/ResendVerifyEmailForm.vue';
  import { RouterLink } from 'vue-router';

  const contactEmailAddress = import.meta.env.VITE_SENDING_EMAIL;
  const emailLink = `mailto:${contactEmailAddress}`;
</script>

<template>
  <main>
    <div class="container">
      <div>
        <h2>Account Unverified</h2>
      </div>
      <div class="message-container">
        <p>Oh shoot! It looks like you have not verified your account</p>
        <p>
          Please check your email inbox for a verification email from
          <a :href="emailLink">{{ contactEmailAddress }}</a
          >. After verifying your account, you should be able to
          <RouterLink to="/masses/login">login</RouterLink> to your account.
        </p>
        <p>
          If you cannot find the email take a peek in your spam folder. If necessary you can use the
          form below to request a new verification email. Note requesting new verification email
          will invalidate the previous verification email.
        </p>
        <ResendVerifyEmailForm />
        <p>
          If you still encounter any issues, please contact us at
          <a :href="emailLink">{{ contactEmailAddress }}</a> for assistance.
        </p>
      </div>
    </div>
  </main>
</template>

<style scoped>
  main {
    display: flex;
    justify-content: center;
    width: 100%;
  }

  .container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  .message-container {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  @media (min-width: 1024px) {
    main {
      flex: 1;
    }
  }
</style>

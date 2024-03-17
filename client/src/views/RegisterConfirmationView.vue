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
        <h2>Registration Successful</h2>
      </div>
      <div class="message-container">
        <p>Oh yeah! Your registration was successful.</p>
        <p>
          Please check your email inbox shortly for a verification email from
          <a :href="emailLink">{{ contactEmailAddress }}</a
          >. After verifying your account, you should be able to
          <RouterLink to="/masses/login">login</RouterLink> to your account.
        </p>
        <p>
          If you do not receive the email within the next few minutes, please check your spam
          folder. You can also use the form below to request a new verification email. Note
          requesting new verification email will invalidate the previous verification email.
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

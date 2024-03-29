<script setup lang="ts">
  import { ref } from 'vue';
  import { useRoute } from 'vue-router';

  type VerificationStatus =
    | 'Not Started'
    | 'Retrieving Token'
    | 'Verifying Token'
    | 'Verified'
    | 'Unverified';

  const verificationStatus = ref<VerificationStatus>('Not Started');
  const route = useRoute();
  const query = route.query;
  const token = query['t'];

  setTimeout(() => {
    verificationStatus.value = 'Retrieving Token';

    setTimeout(() => {
      verificationStatus.value = 'Verifying Token';

      setTimeout(() => {
        verificationStatus.value = 'Unverified';
      }, 2000);
    }, 2000);
  }, 2000);
</script>

<template>
  <main>
    <div class="container">
      <h2>Account Verification</h2>
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
        <div>
          <span v-if="verificationStatus === 'Not Started'">
            Starting verification process...
          </span>
          <span v-if="verificationStatus === 'Retrieving Token'"> Retrieving Token... </span>
          <span v-if="verificationStatus === 'Verifying Token'"> Verifying Token... </span>
          <span v-if="verificationStatus === 'Verified'">
            Successfully verified account! Redirecting...
          </span>
          <span v-if="verificationStatus === 'Unverified'">
            Uh oh! We were unable to verify your account. Please try again.
          </span>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
  main {
    display: flex;
    justify-content: center;
  }

  .container {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 100%;
    gap: 2rem;
  }

  .verification-container {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
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

  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }

  @media (min-width: 1024px) {
    main {
      flex: 1;
      width: 100%;
    }
  }
</style>

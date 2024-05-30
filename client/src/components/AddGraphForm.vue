<script setup lang="ts">
  import type { FormFieldState } from '@/types';
  import { useGraphsService } from '@/composables/useGraphsService.js';
  import { useMounted } from '@/composables/useMounted.js';
  import { useSubmitting } from '@/composables/useSubmitting';
  import { useUserStore } from '@/stores/userStore.js';
  import { toTitleCase } from '@/utils';
  import { reactive } from 'vue';
  import { useRouter } from 'vue-router';

  type AddGraphFormState = {
    fields: Record<'name' | 'apiKey', FormFieldState>;
    errors: string[];
  };

  const formState = reactive<AddGraphFormState>({
    fields: {
      name: {
        value: '',
        errorMessage: '',
      },
      apiKey: {
        value: '',
        errorMessage: '',
      },
    },
    errors: [],
  });

  const { isSubmitting, setIsSubmitting } = useSubmitting();
  const isMounted = useMounted();
  const userStore = useUserStore();
  const graphsService = useGraphsService(userStore);
  const router = useRouter();

  async function handleAddGraphFormSubmit() {
    formState.errors = [];
    setIsSubmitting(true);

    const keys = Object.keys(formState.fields) as (keyof AddGraphFormState['fields'])[];

    for (const key of keys) {
      const field = formState.fields[key];

      if (!field.value) {
        field.errorMessage = `${toTitleCase(key)} is required.`;
        continue;
      }

      field.errorMessage = '';
    }

    const formStateHasError = Object.values(formState.fields).some(field => field.errorMessage);

    if (formStateHasError) {
      setIsSubmitting(false);
      return;
    }

    const addGraphResult = await graphsService.addGraph(
      formState.fields.name.value,
      formState.fields.apiKey.value
    );

    if (addGraphResult.err) {
      setIsSubmitting(false);
      formState.errors.push(...addGraphResult.val.map(err => err.message));
      return;
    }

    setIsSubmitting(false);
    router.push(`/graphs/${addGraphResult.val.id}`);
  }

  function handleInputChange(e: Event) {
    const target = e.target as HTMLInputElement;
    const { id } = target;
    const fieldKey = id as keyof AddGraphFormState['fields'];
    const error = formState.fields[fieldKey].errorMessage;

    if (error) {
      formState.fields[fieldKey].errorMessage = '';
    }
  }
</script>

<template>
  <div class="form-container">
    <h2>Add Graph</h2>
    <form
      aria-label="add graph form"
      class="add-graph-form"
      novalidate
      @submit.prevent="handleAddGraphFormSubmit"
    >
      <div class="form-group">
        <label for="name">Name</label>
        <input
          :disabled="isMounted === false"
          type="text"
          id="name"
          name="name"
          required
          v-model="formState.fields.name.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.name.errorMessage }"
        />
        <span class="error-message" v-if="formState.fields.name.errorMessage">
          {{ formState.fields.name.errorMessage }}
        </span>
      </div>
      <div class="form-group">
        <label for="apiKey">API Key</label>
        <input
          :disabled="isMounted === false"
          type="password"
          id="apiKey"
          name="apiKey"
          required
          v-model="formState.fields.apiKey.value"
          @input="handleInputChange"
          :class="{ invalid: formState.fields.apiKey.errorMessage }"
        />
        <span class="error-message" v-if="formState.fields.apiKey.errorMessage">
          {{ formState.fields.apiKey.errorMessage }}
        </span>
      </div>
      <button :disabled="isMounted === false || isSubmitting" class="add-button" type="submit">
        Add
      </button>
      <ul class="form-errors">
        <li class="error-message" v-for="error in formState.errors" :key="error">{{ error }}</li>
      </ul>
    </form>
  </div>
</template>

<style scoped>
  .form-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding: 2rem;
    border-radius: 1rem;
    background-color: var(--color-background-soft);
    width: 100%;
    max-width: 600px;
  }

  .add-graph-form {
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

  .add-button {
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

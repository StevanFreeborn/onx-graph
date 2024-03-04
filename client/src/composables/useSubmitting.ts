import { readonly, ref } from 'vue';

export function useSubmitting() {
  const isSubmitting = ref<boolean>(false);

  function setIsSubmitting(value: boolean) {
    isSubmitting.value = value;
  }

  return {
    isSubmitting: readonly(isSubmitting),
    setIsSubmitting,
  };
}

import { onMounted, onUnmounted, readonly, ref } from 'vue';

export function useMounted() {
  const isMounted = ref<boolean>(false);

  onMounted(() => {
    isMounted.value = true;
  });

  onUnmounted(() => {
    isMounted.value = false;
  });

  return readonly(isMounted);
}

import { onMounted, onUnmounted, ref } from 'vue';

export function useTheme() {
  const theme = ref<'light' | 'dark'>('light');

  function updateTheme(newTheme: 'light' | 'dark') {
    theme.value = newTheme;
  }

  function handleMediaChange(e: MediaQueryListEvent) {
    updateTheme(e.matches ? 'dark' : 'light');
  }

  onMounted(() => {
    const isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    updateTheme(isDark ? 'dark' : 'light');

    window
      .matchMedia('(prefers-color-scheme: dark)')
      .addEventListener('change', handleMediaChange);
  });

  onUnmounted(() => {
    window
      .matchMedia('(prefers-color-scheme: dark)')
      .removeEventListener('change', handleMediaChange);
  });

  return theme;
}

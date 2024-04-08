import { fileURLToPath } from 'node:url';
import { configDefaults, defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config';

export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      poolOptions: {
        vmThreads: {
          minThreads: process.env.CI ? 4 : undefined,
          maxThreads: process.env.CI ? 4 : undefined,
          memoryLimit: process.env.CI ? 0.5 : undefined,
        },
      },
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/**/*'],
      root: fileURLToPath(new URL('./', import.meta.url)),
      coverage: {
        provider: 'istanbul',
        reportsDirectory: './tests/coverage',
        include: ['**/src/**'],
        exclude: ['**/views/**', '**/src/App.vue', '**/src/main.ts', '**/router/index.ts'],
        reporter: ['text', 'html'],
      },
    },
  })
);

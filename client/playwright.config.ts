import { defineConfig, devices } from '@playwright/test';
import { env } from './e2e/env';

export default defineConfig({
  reportSlowTests: null,
  testDir: './e2e',
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  forbidOnly: !!env.CI,
  retries: env.CI ? 2 : 0,
  workers: env.CI ? 1 : undefined,
  reporter: env.CI ? [['blob'], ['github'], ['list'], ['html']] : [['html'], ['list']],
  use: {
    actionTimeout: 0,
    baseURL: 'http://localhost:4001',
    trace: 'retain-on-failure',
    headless: true,
    viewport: { width: 1920, height: 1080 },
    navigationTimeout: 0,
  },
  projects: [
    {
      name: 'docker-down',
      testMatch: '**/cleanup/docker-down.ts',
      dependencies: ['chrome', 'firefox', 'edge'],
    },
    {
      name: 'chrome',
      use: {
        ...devices['Desktop Chrome'],
      },
    },
    {
      name: 'firefox',
      use: {
        ...devices['Desktop Firefox'],
      },
    },
    {
      name: 'edge',
      use: {
        ...devices['Desktop Edge'],
      },
    },
  ],
  webServer: [
    {
      command: 'npm run test:e2e:stack',
      url: 'http://localhost:4001',
      reuseExistingServer: !env.CI,
    },
  ],
});

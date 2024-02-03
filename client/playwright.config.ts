import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  reportSlowTests: null,
  testDir: './e2e',
  timeout: 30 * 1000,
  expect: {
    timeout: 5000,
  },
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: process.env.CI ? [['blob'], ['github'], ['list']] : [['html'], ['list']],
  use: {
    actionTimeout: 0,
    baseURL: 'https://localhost:5173',
    trace: 'retain-on-failure',
    headless: true,
    viewport: { width: 1920, height: 1080 },
  },
  projects: [
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
      command: process.env.CI ? 'vite preview --port 5173' : 'vite dev',
      port: 5173,
      reuseExistingServer: !process.env.CI,
    },
    {
      command: 'dotnet run --project ../server/Server.API/Server.API.csproj',
      reuseExistingServer: !process.env.CI,
    },
  ],
});

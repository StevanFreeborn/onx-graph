import AxeBuilder from '@axe-core/playwright';
import { test as base } from '@playwright/test';
import { AxeResults } from 'axe-core';

type TestUser = {
  email: string;
  password: string;
  username: string;
};

type GlobalFixtures = {
  accessibilityResults: AxeResults;
  user: TestUser;
};

export const test = base.extend<GlobalFixtures>({
  accessibilityResults: async ({ page }, use: (r: AxeResults) => Promise<void>, testInfo) => {
    const results = await new AxeBuilder({ page }).disableRules('color-contrast').analyze();

    await use(results);

    await testInfo.attach('accessibility-scan-results', {
      body: JSON.stringify(results, null, 2),
      contentType: 'application/json',
    });
  },
  user: async ({}, use) =>
    await use({
      email: 'test@test.com',
      password: '@Password1',
      username: 'test',
    }),
});

export * from '@playwright/test';

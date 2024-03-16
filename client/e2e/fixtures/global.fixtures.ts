import AxeBuilder from '@axe-core/playwright';
import { test as base } from '@playwright/test';
import { AxeResults } from 'axe-core';
import mailhog from 'mailhog';
import { env } from '../env';

type TestUser = {
  email: string;
  password: string;
  username: string;
};

type GlobalFixtures = {
  accessibilityResults: AxeResults;
  user: TestUser;
  mailhog: mailhog.API;
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
      email: env.PW_TEST_USER_EMAIL,
      password: env.PW_TEST_USER_PASSWORD,
      username: env.PW_TEST_USER_USERNAME,
    }),
  mailhog: async ({}, use) => await use(mailhog({ port: env.PW_MAILHOG_PORT })),
});

export * from '@playwright/test';

import AxeBuilder from '@axe-core/playwright';
import { Page, test as base } from '@playwright/test';
import { AxeResults } from 'axe-core';
import mailhog from 'mailhog';
import { Db, MongoClient } from 'mongodb';
import { env } from '../env';

type TestUser = {
  email: string;
  password: string;
  username: string;
};

type GlobalFixtures = {
  page: Page;
  accessibilityResults: AxeResults;
  user: TestUser;
  mailhog: mailhog.API;
  dbcontext: Db;
};

export const test = base.extend<GlobalFixtures>({
  page: async ({ browser }, use) => {
    const context = await browser.newContext();
    const page = await context.newPage();
    await use(page);
    await page.close();
  },
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
  dbcontext: async ({}, use) => {
    const mongoClient = new MongoClient(env.PW_DB_CONNECTION_STRING);
    await mongoClient.connect();

    const dbName = env.PW_DB_CONNECTION_STRING.split('/')?.pop()?.split('?').shift();

    await use(mongoClient.db(dbName));
    await mongoClient.close();
  },
});

export * from '@playwright/test';

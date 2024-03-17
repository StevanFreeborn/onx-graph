import AxeBuilder from '@axe-core/playwright';
import { faker } from '@faker-js/faker';
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

type NewUser = Omit<TestUser, 'username'>;

type GlobalFixtures = {
  page: Page;
  accessibilityResults: AxeResults;
  verifiedUser: TestUser;
  unverifiedUser: TestUser;
  newUser: NewUser;
  mailhog: mailhog.API;
  dbcontext: Db;
};

const testPassword = '@Password1';
const testPasswordHash = '$2a$11$Xs1mALyCfYD7Er2542tlVupp7GnXIwj5kA/0e6d1Dapws80QwuWoq';

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
  newUser: async ({}, use) => await use({ email: faker.internet.email(), password: testPassword }),
  dbcontext: async ({}, use) => {
    const mongoClient = new MongoClient(env.PW_DB_CONNECTION_STRING);
    await mongoClient.connect();

    const dbName = env.PW_DB_CONNECTION_STRING.split('/')?.pop()?.split('?').shift();

    await use(mongoClient.db(dbName));
    await mongoClient.close();
  },
  verifiedUser: async ({ dbcontext }, use) => {
    const user = {
      _id: faker.database.mongodbObjectId(),
      email: faker.internet.email(),
      username: faker.internet.userName(),
      password: testPasswordHash,
      createdAt: new Date(),
      updatedAt: new Date(),
      isVerified: true,
    };

    await dbcontext.collection('users').insertOne(user as any);

    await use({ email: user.email, password: testPassword, username: user.username });

    await dbcontext.collection('users').deleteOne({ email: user.email });
  },
  unverifiedUser: async ({ dbcontext }, use) => {
    const user = {
      _id: faker.database.mongodbObjectId(),
      email: faker.internet.email(),
      username: faker.internet.userName(),
      password: testPasswordHash,
      createdAt: new Date(),
      updatedAt: new Date(),
      isVerified: false,
    };

    await dbcontext.collection('users').insertOne(user as any);

    await use({ email: user.email, password: testPassword, username: user.username });

    await dbcontext.collection('users').deleteOne({ email: user.email });
  },
  mailhog: async ({}, use) => await use(mailhog({ port: env.PW_MAILHOG_PORT })),
});

export * from '@playwright/test';

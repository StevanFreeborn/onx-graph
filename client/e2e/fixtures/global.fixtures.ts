import AxeBuilder from '@axe-core/playwright';
import { faker } from '@faker-js/faker';
import { Page, TestInfo, test as base } from '@playwright/test';
import { AxeResults } from 'axe-core';
import mailhog from 'mailhog';
import { Db, MongoClient } from 'mongodb';
import { env } from '../env';

type TestUser = {
  email: string;
  password: string;
  username: string;
};

type VerifiedUser = TestUser;

type UnverifiedUser = TestUser & { verificationToken: string };

type NewUser = Omit<TestUser, 'username'>;

type AuthenticatedUserPage = Page;

type GlobalFixtures = {
  page: Page;
  getAccessibilityResults: (page: Page, testInfo: TestInfo) => Promise<AxeResults>;
  verifiedUser: VerifiedUser;
  unverifiedUser: UnverifiedUser;
  newUser: NewUser;
  mailhog: mailhog.API;
  dbcontext: Db;
  authenticatedUserPage: AuthenticatedUserPage;
};

const testPassword = '@Password1';
const testPasswordHash = '$2a$11$Xs1mALyCfYD7Er2542tlVupp7GnXIwj5kA/0e6d1Dapws80QwuWoq';
const testUserEncryptionKey = 'jm4FmaUQy6siVjtKrui4mgP6VqD9ITqhW27hiMg2hDw=';
// const testEncryptedApiKey = '6iOGOCLgeHeuANqNBFqCsg==';

export const test = base.extend<GlobalFixtures>({
  page: async ({ browser }, use) => {
    const context = await browser.newContext();
    const page = await context.newPage();
    await use(page);
    await page.close();
  },
  getAccessibilityResults: async (
    {},
    use: (r: (page: Page, testInfo: TestInfo) => Promise<AxeResults>) => Promise<void>
  ) =>
    await use(async (page, testInfo) => {
      const results = await new AxeBuilder({ page }).disableRules('color-contrast').analyze();
      await testInfo.attach('accessibility-scan-results', {
        body: JSON.stringify(results, null, 2),
        contentType: 'application/json',
      });
      return results;
    }),
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
      encryptionKey: testUserEncryptionKey,
    };

    await dbcontext.collection('users').insertOne(user as any);

    await use({ email: user.email, password: testPassword, username: user.username });

    await cleanupUser(dbcontext, user);
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
      encryptionKey: testUserEncryptionKey,
    };

    const FIFTEEN_MINUTES = 1000 * 60 * 15;

    const verificationToken = {
      _id: faker.database.mongodbObjectId(),
      userId: user._id,
      token: faker.string.uuid(),
      expiresAt: new Date(Date.now() + FIFTEEN_MINUTES),
      revoked: false,
      tokenType: 1,
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    await dbcontext.collection('users').insertOne(user as any);
    await dbcontext.collection('tokens').insertOne(verificationToken as any);

    await use({
      email: user.email,
      password: testPassword,
      username: user.username,
      verificationToken: verificationToken.token,
    });

    await cleanupUser(dbcontext, user);
  },
  mailhog: async ({}, use) => await use(mailhog({ port: env.PW_MAILHOG_PORT })),
  authenticatedUserPage: async ({ verifiedUser, page }, use) => {
    await page.goto('/masses/login');

    await page.getByLabel('Email').fill(verifiedUser.email);
    await page.getByLabel('Password').fill(verifiedUser.password);
    await page.getByRole('button', { name: 'Login' }).click();

    await page.waitForURL(/\/graphs/, { timeout: 30000 });

    await use(page);
  },
});

async function cleanupUser(dbcontext: Db, user: { email: string; _id: string }) {
  await dbcontext.collection('users').deleteOne({ email: user.email });
  await dbcontext.collection('tokens').deleteMany({ userId: user._id });
  await dbcontext.collection('graphs').deleteMany({ userId: user._id });
}

export * from '@playwright/test';

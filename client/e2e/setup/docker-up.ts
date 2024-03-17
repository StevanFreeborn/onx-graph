/* eslint-disable playwright/no-standalone-expect */
import { expect, test as setup } from '@playwright/test';
import { exec } from 'child_process';

setup('docker-compose up', async ({ request }) => {
  await setup.step('execute docker-compose up', async () => {
    const dockerComposePath = `${process.cwd()}/docker-compose.test.yml`;
    exec(`docker compose -f ${dockerComposePath} up --build`);
  });

  await setup.step('wait for server to be ready', async () => {
    const baseUrl = setup.info().project.use.baseURL;

    expect(baseUrl).toBeDefined();

    await expect(async () => {
      const response = await request.get(baseUrl!);
      expect(response.ok()).toBeTruthy();
    }).toPass({ timeout: 120000 });
  });
});

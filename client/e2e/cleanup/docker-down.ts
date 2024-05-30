import { test as cleanup } from '@playwright/test';
import { exec } from 'child_process';

cleanup('docker-compose down', () => {
  const dockerComposePath = `${process.cwd()}/docker-compose.test.yml`;
  exec(`docker compose --progress quiet -f ${dockerComposePath} down`);
});

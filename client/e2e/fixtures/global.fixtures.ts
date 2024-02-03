import AxeBuilder from '@axe-core/playwright';
import { test as base } from '@playwright/test';
import { AxeResults } from 'axe-core';

type GlobalFixtures = {
  accessibilityResults: AxeResults;
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
});

export * from '@playwright/test';

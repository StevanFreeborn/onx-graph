import { expect, test } from '../fixtures/global.fixtures';

test.describe('VerifyAccountView', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/masses/verify-account');
  });

  test('when user visits verify account it should pass all accessibility tests', async ({
    page,
    accessibilityResults,
  }) => {
    await page.waitForURL(/masses\/verify-account/);
    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits verify account they should see a heading', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /account verification/i })).toBeVisible();
  });
});

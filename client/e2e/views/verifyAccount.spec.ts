import { expect, test } from '../fixtures/global.fixtures';

test.describe('VerifyAccountView', () => {
  test('when user visits verify account and verification fails it should pass all accessibility tests', async ({
    page,
    getAccessibilityResults,
  }) => {
    await page.goto('/masses/verify-account');
    await expect(page.getByText(/we were unable to verify your account/i)).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits verify account they should see a heading', async ({ page }) => {
    await page.goto('/masses/verify-account');
    await expect(page.getByRole('heading', { name: /account verification/i })).toBeVisible();
  });

  test('when user visits verify account and verification fails it should display an error message', async ({
    page,
  }) => {
    test.setTimeout(70000);

    await page.goto('/masses/verify-account?t=invalid-token');
    await expect(page.getByText(/we were unable to verify your account/i)).toBeVisible({
      timeout: 60000,
    });
  });

  test('when user visits verify account and verification succeeds it should redirect to login', async ({
    page,
    unverifiedUser,
  }) => {
    test.setTimeout(70000);

    await page.goto(`/masses/verify-account?t=${unverifiedUser.verificationToken}`);
    await page.waitForURL('/masses/login', { timeout: 60000 });

    expect(page.url()).toMatch(/masses\/login/);
  });
});

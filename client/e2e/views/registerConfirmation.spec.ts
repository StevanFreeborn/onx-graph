import { expect, test } from '../fixtures/global.fixtures.js';

test.describe('RegisterConfirmationView', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/masses/register-confirmation');
  });

  test('when user visits register confirmation it should pass all accessibility tests', async ({
    accessibilityResults,
  }) => {
    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits register confirmation they should see a heading', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /registration successful/i })).toBeVisible();
  });

  test('when user visits register confirmation they should see a message', async ({ page }) => {
    await expect(page.getByText(/your registration was successful/i)).toBeVisible();
  });

  test('when user visits register confirmation they should see a link to the login page', async ({
    page,
  }) => {
    const link = page.getByRole('link', { name: /login/i }).last();

    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/masses/login');
  });

  test('when user clicks on the login link they should be taken to the login page', async ({
    page,
  }) => {
    const link = page.getByRole('link', { name: /login/i }).last();
    await link.click();
    await expect(page).toHaveURL(/masses\/login/);
  });

  test('when user visits registration confirmation they should be informed of email address used for sending verification email', async ({
    page,
  }) => {
    const email = page.getByRole('link', { name: /.+@.+\..+/i }).first();

    await expect(email).toBeVisible();
  });

  test('when user visits registration confirmation they should see a form to resend verification email', async ({
    page,
  }) => {
    const form = page.getByRole('form');

    await expect(form).toBeVisible();
  });
});

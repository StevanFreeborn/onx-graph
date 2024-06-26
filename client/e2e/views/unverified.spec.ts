import { expect, test } from '../fixtures/global.fixtures';

test.describe('UnverifiedView', () => {
  test('when user visits unverified it should pass all accessibility tests', async ({
    page,
    getAccessibilityResults,
  }) => {
    await page.goto('/masses/unverified');

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits unverified they should see a heading', async ({ page }) => {
    await page.goto('/masses/unverified');
    await expect(page.getByRole('heading', { name: /account unverified/i })).toBeVisible();
  });

  test('when user visits unverified confirmation they should see a message', async ({ page }) => {
    await page.goto('/masses/unverified');
    await expect(page.getByText(/you have not verified your account/i)).toBeVisible();
  });

  test('when user visits register confirmation they should be told the verification link expired after 15 minutes', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');
    await expect(page.getByText(/links expire after 15 minutes/i)).toBeVisible();
  });

  test('when user visits register confirmation they should be told if they are unverified after 48 hours their account will be deleted', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');
    await expect(page.getByText(/not verified within 48 hours/i)).toBeVisible();
    await expect(page.getByText(/will have been deleted/i)).toBeVisible();
  });

  test('when user visits unverified they should see a link to the login page', async ({ page }) => {
    await page.goto('/masses/unverified');

    const link = page.getByRole('link', { name: /login/i }).last();

    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/masses/login');
  });

  test('when user clicks on the login link they should be taken to the login page', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');
    const link = page.getByRole('link', { name: /login/i }).last();
    await link.click();
    await expect(page).toHaveURL(/masses\/login/);
  });

  test('when user visits unverified they should be informed of email address used for sending verification email', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');

    const email = page.getByRole('link', { name: /.+@.+\..+/i }).first();

    await expect(email).toBeVisible();
  });

  test('when user visits unverified they should see a form to resend verification email', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');

    const form = page.getByRole('form');

    await expect(form).toBeVisible();
  });

  test('when user does not enter an email and submits the form to resend verification email then they should see an error message', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');

    const submit = page.getByRole('button', { name: /resend/i });

    await submit.click();

    await expect(page.getByText(/email is required/i)).toBeVisible();
  });

  test('when user enters an invalid email and submits the form to resend verification email then they should see an error message', async ({
    page,
  }) => {
    await page.goto('/masses/unverified');

    const email = page.getByRole('textbox', { name: /email/i });
    const submit = page.getByRole('button', { name: /resend/i });

    await email.fill('test');
    await submit.click();

    await expect(page.getByText(/enter a valid email/i)).toBeVisible();
  });

  test('when user fills out the form to resend verification email and submits it then they should see a success message', async ({
    page,
    unverifiedUser,
  }) => {
    await page.goto('/masses/unverified');

    const email = page.getByRole('textbox', { name: /email/i });
    const submit = page.getByRole('button', { name: /resend/i });

    await email.fill(unverifiedUser.email);
    await submit.click();

    await expect(page.getByText(/email sent successfully/i)).toBeVisible();
  });
});

import { expect, test } from '../fixtures/global.fixtures.js';

test.describe('RegisterView', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/masses/register');
  });

  test('when user visits register it should pass all accessibility tests', async ({
    accessibilityResults,
  }) => {
    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits register they should see a heading', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Register' })).toBeVisible();
  });

  test('when user visits register the should see a form', async ({ page }) => {
    await expect(page.getByRole('form')).toBeVisible();
  });

  test('when user visits register they should see an email input', async ({ page }) => {
    await expect(page.getByLabel('Email')).toBeVisible();
  });

  test('when user visits register they should see a password input', async ({ page }) => {
    await expect(page.getByLabel('Password', { exact: true })).toBeVisible();
  });

  test('when user visits register they should see a confirm password input', async ({ page }) => {
    await expect(page.getByLabel('Confirm Password')).toBeVisible();
  });

  test('when user visits register they should see a register button', async ({ page }) => {
    await expect(page.getByRole('button', { name: 'Register' })).toBeVisible();
  });

  test('when user visits register they should see a link to the login page', async ({ page }) => {
    const link = page.getByText('Login');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/masses/login');
  });

  test('when user clicks on the login link they should be taken to the login page', async ({
    page,
  }) => {
    const link = page.getByText('Login');
    await link.click();
    await page.waitForURL(/masses\/login/);
    expect(page.url()).toMatch(/masses\/login/);
  });

  test('when user submits the register form with no email they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Password', { exact: true }).fill('@Password123');
    await page.getByLabel('Confirm Password').fill('@Password123');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Email is required/)).toBeVisible();
  });

  test('when user submits the register form with no password they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByLabel('Confirm Password').fill('@Password123');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Password is required/)).toBeVisible();
  });

  test('when user submits the register form with no confirm password they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByLabel('Password', { exact: true }).fill('@Password123');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Confirm Password is required/)).toBeVisible();
  });

  test('when user submits the register form with non matching passwords they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByLabel('Password', { exact: true }).fill('@Password12');
    await page.getByLabel('Confirm Password').fill('@Password123');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Passwords do not match/)).toBeVisible();
  });

  test('when user submits the register form with invalid email they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Email').fill('test.user');
    await page.getByLabel('Password', { exact: true }).fill('@Password123');
    await page.getByLabel('Confirm Password').fill('@Password123');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Enter a valid email address/)).toBeVisible();
  });

  test('when user submits the register form with invalid password they should see an error message', async ({
    page,
  }) => {
    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByLabel('Password', { exact: true }).fill('password');
    await page.getByLabel('Confirm Password').fill('password');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page.getByText(/Password must be/)).toBeVisible();
  });

  test('when user submits the register form with valid data they should be redirected to the login page', async ({
    page,
    newUser,
  }) => {
    await page.getByLabel('Email').fill(newUser.email);
    await page.getByLabel('Password', { exact: true }).fill(newUser.password);
    await page.getByLabel('Confirm Password').fill('@Password1');
    await page.getByRole('button', { name: 'Register' }).click();
    await expect(page).toHaveURL(/masses\/register-confirmation/, { timeout: 30000 });
  });
});

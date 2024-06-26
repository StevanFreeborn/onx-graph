import { expect, test } from '../fixtures/global.fixtures';

test.describe('LoginView', () => {
  test('when user visits login it should pass all accessibility tests', async ({
    page,
    getAccessibilityResults,
  }) => {
    await page.goto('/masses/login');

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits login they should see a heading', async ({ page }) => {
    await page.goto('/masses/login');

    await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();
  });

  test('when user visits login the should see a form', async ({ page }) => {
    await page.goto('/masses/login');

    await expect(page.getByRole('form')).toBeVisible();
  });

  test('when user visits login they should see an email input', async ({ page }) => {
    await page.goto('/masses/login');

    await expect(page.getByLabel('Email')).toBeVisible();
  });

  test('when user visits login they should see a password input', async ({ page }) => {
    await page.goto('/masses/login');

    await expect(page.getByLabel('Password')).toBeVisible();
  });

  test('when user visits login they should see a login button', async ({ page }) => {
    await page.goto('/masses/login');

    await expect(page.getByRole('button', { name: 'Login' })).toBeVisible();
  });

  test('when user visits login they should see a link to the register page', async ({ page }) => {
    await page.goto('/masses/login');

    const link = page.getByText('Register');

    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/masses/register');
  });

  test('when user clicks on the register link they should be taken to the register page', async ({
    page,
  }) => {
    await page.goto('/masses/login');

    const link = page.getByText('Register');

    await link.click();

    await page.waitForURL(/masses\/register/);
    expect(page.url()).toMatch(/masses\/register/);
  });

  test('when user submits the login form with no email they should see an error message', async ({
    page,
  }) => {
    await page.goto('/masses/login');

    await page.getByLabel('Password').fill('@Password123');
    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page.getByText(/Email is required/)).toBeVisible();
  });

  test('when user submits the login form with no password they should see an error message', async ({
    page,
  }) => {
    await page.goto('/masses/login');

    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page.getByText(/Password is required/)).toBeVisible();
  });

  test('when user submits the login form with invalid credentials they should see an error message', async ({
    page,
  }) => {
    await page.goto('/masses/login');

    await page.getByLabel('Email').fill('test.user@gmail.com');
    await page.getByLabel('Password').fill('Password123');
    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page.getByText(/Email\/Password combination is not valid/)).toBeVisible();
  });

  test('when user submits the login form with valid credentials they should be redirected to the graphs page', async ({
    page,
    verifiedUser: user,
  }) => {
    await page.goto('/masses/login');

    await page.getByLabel('Email').fill(user.email);
    await page.getByLabel('Password').fill(user.password);
    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page).toHaveURL(/\/graphs/, { timeout: 30000 });
  });

  test('when user submits the login form with valid credentials but they have not verified their account they should be redirect to the unverified page', async ({
    page,
    unverifiedUser: user,
  }) => {
    await page.goto('/masses/login');

    await page.getByLabel('Email').fill(user.email);
    await page.getByLabel('Password').fill(user.password);
    await page.getByRole('button', { name: 'Login' }).click();

    await expect(page).toHaveURL(/\/masses\/unverified/, { timeout: 30000 });
  });
});

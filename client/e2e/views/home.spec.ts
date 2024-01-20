import { expect, test } from '@playwright/test';

test.describe('HomeView', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('when user visits / they should see an OnxGraph heading', async ({
    page,
  }) => {
    await expect(page.getByText('OnxGraph')).toBeVisible();
  });

  test('when user visits / they should see a description of the site', async ({
    page,
  }) => {
    const expectedDescription =
      'Create visualizations of your Onspring relationships';
    await expect(page.getByText(expectedDescription)).toBeVisible();
  });

  test('when user visits / they should see a link to the Onspring website', async ({
    page,
  }) => {
    const link = page.getByText('Onspring');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', 'https://onspring.com/');
  });

  test('when user visits / they should see a link to the login page', async ({
    page,
  }) => {
    const link = page.getByText('Login');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/login');
  });

  test('when user visits / they should see a link to the register page', async ({
    page,
  }) => {
    const link = page.getByText('Register');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/register');
  });

  test('when user visits / they should see demo graph', async ({ page }) => {
    const graph = page.getByTestId('demo-graph');
    await expect(graph).toBeVisible();
  });
});

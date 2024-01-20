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

  test('when user clicks on the login link they should be taken to the login page', async ({
    page,
  }) => {
    const link = page.getByText('Login');
    await link.click();
    await page.waitForURL(/login/);
    expect(page.url()).toMatch(/login/);
  });

  test('when user visits / they should see a link to the register page', async ({
    page,
  }) => {
    const link = page.getByText('Register');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/register');
  });

  test('when user clicks on the register link they should be taken to the register page', async ({
    page,
  }) => {
    const link = page.getByText('Register');
    await link.click();
    await page.waitForURL(/register/);
    expect(page.url()).toMatch(/register/);
  });

  test('when user visits / they should see demo graph', async ({ page }) => {
    const graph = page.getByTestId('demo-graph');
    await expect(graph).toBeVisible();
  });

  test('when user visits / on a mobile device they should see the heading above the demo graph', async ({
    page,
  }) => {
    const heading = page.getByText('OnxGraph');
    const graph = page.getByTestId('demo-graph');

    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/');

    const headingBox = await heading.boundingBox();
    const graphBox = await graph.boundingBox();

    expect(headingBox?.y ?? 0).toBeLessThan(graphBox?.y ?? -1);
  });
});

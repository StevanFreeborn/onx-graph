import { expect, test } from '../fixtures/global.fixtures.js';

test.describe('HomeView', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/masses/home');
  });

  test('when user visits home it should pass all accessibility tests', async ({
    accessibilityResults,
  }) => {
    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits home they should see an OnxGraph heading', async ({ page }) => {
    await expect(page.getByText('OnxGraph')).toBeVisible();
  });

  test('when user visits home they should see a description of the site', async ({ page }) => {
    const expectedDescription = 'Create visualizations of your Onspring relationships';
    await expect(page.getByText(expectedDescription)).toBeVisible();
  });

  test('when user visits home they should see a link to the Onspring website', async ({ page }) => {
    const link = page.getByText('Onspring');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', 'https://onspring.com/');
  });

  test('when user visits home they should see a link to the login page', async ({ page }) => {
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

  test('when user visits home they should see a link to the register page', async ({ page }) => {
    const link = page.getByText('Register');
    await expect(link).toBeVisible();
    await expect(link).toHaveAttribute('href', '/masses/register');
  });

  test('when user clicks on the register link they should be taken to the register page', async ({
    page,
  }) => {
    const link = page.getByText('Register');
    await link.click();
    await page.waitForURL(/masses\/register/);
    expect(page.url()).toMatch(/masses\/register/);
  });

  test('when user visits home they should see demo graph', async ({ page }) => {
    const graph = page.getByTestId('demo-graph');
    await expect(graph).toBeVisible();
  });

  test('when user visits home on a mobile device they should see the heading above the demo graph', async ({
    page,
  }) => {
    const heading = page.getByText('OnxGraph');
    const graph = page.getByTestId('demo-graph');

    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/masses/home');

    const headingBox = await heading.boundingBox();
    const graphBox = await graph.boundingBox();

    expect(headingBox?.y ?? 0).toBeLessThan(graphBox?.y ?? -1);
  });
});

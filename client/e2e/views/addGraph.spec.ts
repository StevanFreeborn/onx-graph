import { expect, test } from '../fixtures/global.fixtures';

test.describe('AddGraphView', () => {
  test('when user visits add graph it should pass all accessibility tests', async ({
    authenticatedUserPage: page,
    getAccessibilityResults,
  }) => {
    await page.goto('/graphs/add');

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits add graph they should see a heading', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await expect(page.getByRole('heading', { name: 'Add Graph' })).toBeVisible();
  });

  test('when user visits add graph they should see a form', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await expect(page.getByRole('form')).toBeVisible();
  });

  test('when user visits add graph they should see a name input', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await expect(page.getByLabel('Name')).toBeVisible();
  });

  test('when user visits add graph they should see a api key input', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await expect(page.getByLabel('API Key')).toBeVisible();
  });

  test('when user submits the form with no name they should see an error message', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await page.getByRole('button', { name: 'Add' }).click();

    await expect(page.getByText('Name is required')).toBeVisible();
  });

  test('when user submits the form with no api key they should see an error message', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs/add');

    await page.getByRole('button', { name: 'Add' }).click();

    await expect(page.getByText('API Key is required')).toBeVisible();
  });

  test("when user submits the form with valid data they should be taken to the graph's page", async ({
    authenticatedUserPage: page,
    dbcontext,
  }) => {
    await page.goto('/graphs/add');

    const nameInput = page.getByLabel('Name');
    const apiKeyInput = page.getByLabel('API Key');
    const addGraphButton = page.getByRole('button', { name: 'Add' });

    const graphName = 'Test Graph';

    await nameInput.fill(graphName);
    await apiKeyInput.fill('test-api-key');
    await addGraphButton.click();

    await expect(page).toHaveURL(new RegExp('/graphs/[a-z0-9]{24}'));

    const graph = await dbcontext.collection('graphs').findOne({ name: graphName });

    expect(graph).not.toBeNull();
  });
});

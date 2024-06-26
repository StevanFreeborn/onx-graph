import { expect, test } from '../fixtures/global.fixtures';

test.describe('GraphsView', () => {
  test('when user visits graphs it should pass all accessibility tests without any graphs', async ({
    authenticatedUserPage: page,
    getAccessibilityResults,
  }) => {
    await page.goto('/graphs');

    const noGraphsMessage = page.getByText(/you don't have any graphs yet/i);

    await expect(noGraphsMessage).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits graphs it should pass all accessibility tests with graphs', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    getAccessibilityResults,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id);
    await page.goto('/graphs');

    const graphName = page.getByText(graph.name);

    await expect(graphName).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits graphs it should display graphs', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id);
    await page.goto('/graphs');

    const graphName = page.getByText(graph.name);

    await expect(graphName).toBeVisible();
  });

  test('when user collapses sidebar it should save the state', async ({
    authenticatedUserPage: page,
  }) => {
    await page.goto('/graphs');

    const collapseButton = page.getByRole('button', { name: /collapse/i });

    await collapseButton.click();

    await page.reload();

    const expandButton = page.getByRole('button', { name: /expand/i });

    await expect(expandButton).toBeVisible();
  });
});

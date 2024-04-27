import { expect, test } from '../fixtures/global.fixtures';

test.describe('GraphsView', () => {
  test('when user visits graphs it should pass all accessibility tests', async ({
    authenticatedUserPage: page,
    getAccessibilityResults,
  }) => {
    await page.goto('/graphs');

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });
});

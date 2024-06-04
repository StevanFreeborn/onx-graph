import { expect, test } from '../fixtures/global.fixtures';

test.describe('GraphView', () => {
  test('when user visits graph it should pass all accessibility tests while wait for updates', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    getAccessibilityResults,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id);
    await page.goto(`/graphs/${graph._id}`);

    const graphName = page.getByLabel(/graph name/i);

    await expect(graphName).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits graph it should pass all accessibility tests when graph is not built', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    getAccessibilityResults,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id, 0);
    await page.goto(`/graphs/${graph._id}`);

    const notBuiltMsg = page.getByText(/has not been built/i);

    await expect(notBuiltMsg).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits graph it should pass all accessibility tests when graph is built', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    getAccessibilityResults,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id, 2);
    await page.goto(`/graphs/${graph._id}`);

    const graphName = page.getByLabel(/graph name/i);

    await expect(graphName).toBeVisible();

    const accessibilityResults = await getAccessibilityResults(page, test.info());

    expect(accessibilityResults.violations.length).toBe(0);
  });

  test('when user visits graph and is building it should display building message and icon', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id, 1);
    await page.goto(`/graphs/${graph._id}`);

    const buildingMsg = page.getByText(/waiting for updates/i);
    const buildingIcon = page.getByTitle(/building/i);

    await expect(buildingMsg).toBeVisible();
    await expect(buildingIcon).toBeVisible();
  });

  test('when user visits graph and is not built it should display not built message and icon', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id, 0);
    await page.goto(`/graphs/${graph._id}`);

    const notBuiltMsg = page.getByText(/has not been built/i);
    const notBuiltIcon = page.getByTitle(/not built/i);

    await expect(notBuiltMsg).toBeVisible();
    await expect(notBuiltIcon).toBeVisible();
  });

  test('when user visits graph and is built it should display built icon', async ({
    verifiedUser: user,
    authenticatedUserPage: page,
    insertFakeGraphForUser: insertGraphForUser,
  }) => {
    const graph = await insertGraphForUser(user.id, 2);
    await page.goto(`/graphs/${graph._id}`);

    const builtIcon = page.getByTitle(/built/i);

    await expect(builtIcon).toBeVisible();
  });
});

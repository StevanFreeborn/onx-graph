import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import PublicLayout from '../../src/components/PublicLayout.vue';
import { renderApp } from '../testUtils';

describe('PublicLayout', () => {
  afterEach(cleanup);

  it('should display a logo', async () => {
    const { getByAltText } = await renderApp(PublicLayout);
    const logo = getByAltText(/logo/);

    expect(logo).toBeInTheDocument();
  });

  it('should display a login link', async () => {
    const { getByRole } = await renderApp(PublicLayout);
    const loginLink = getByRole('link', { name: 'Login' });

    expect(loginLink).toBeInTheDocument();
  });

  it('should display a register link', async () => {
    const { getByRole } = await renderApp(PublicLayout);
    const registerLink = getByRole('link', { name: 'Register' });

    expect(registerLink).toBeInTheDocument();
  });
});

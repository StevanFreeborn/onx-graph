import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import PublicLayout from '../../src/components/PublicLayout.vue';
import { customRender } from '../testUtils';

describe('PublicLayout', () => {
  afterEach(cleanup);

  it('should display a logo', async () => {
    const { getByAltText } = await customRender(PublicLayout);
    const logo = getByAltText(/logo/);

    expect(logo).toBeInTheDocument();
  });

  it('should display a login link', async () => {
    const { getByRole } = await customRender(PublicLayout);
    const loginLink = getByRole('link', { name: 'Login' });

    expect(loginLink).toBeInTheDocument();
  });

  it('should display a register link', async () => {
    const { getByRole } = await customRender(PublicLayout);
    const registerLink = getByRole('link', { name: 'Register' });

    expect(registerLink).toBeInTheDocument();
  });

  it('should render hero greeting component', async () => {
    const { getByText } = await customRender(PublicLayout, {
      global: {
        stubs: {
          HeroGreeting: {
            template: '<div>hero greeting</div>',
          },
        },
      },
    });

    const greeting = getByText(/hero greeting/);

    expect(greeting).toBeInTheDocument();
  });
});

import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import AuthenticatedLayout from '../../src/components/AuthenticatedLayout.vue';
import { customRender } from '../testUtils';

describe('AuthenticatedLayout', () => {
  afterEach(cleanup);

  it('should render the view content within main element', async () => {
    const { getByText, getByRole } = await customRender(AuthenticatedLayout, {
      global: {
        stubs: {
          RouterView: {
            template: '<div>view content</div>',
          },
        },
      },
    });

    const main = getByRole('main');
    const viewContent = getByText(/view content/);

    expect(main).toBeInTheDocument();
    expect(viewContent).toBeInTheDocument();
  });

  it('should render the sidebar in aside element', async () => {
    const { getByText, getByRole } = await customRender(AuthenticatedLayout, {
      global: {
        stubs: {
          NavigationSidebar: {
            template: '<div>sidebar</div>',
          },
        },
      },
    });

    const aside = getByRole('complementary');
    const sidebar = getByText(/sidebar/);

    expect(aside).toBeInTheDocument();
    expect(sidebar).toBeInTheDocument();
  });
});

import '@testing-library/jest-dom/vitest';
import { cleanup, render } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import HeroGreeting from '../../src/components/HeroGreeting.vue';

describe('HeroGreeting', () => {
  afterEach(cleanup);

  it('should a heading that displays OnxGraph', () => {
    const { getByText } = render(HeroGreeting);
    const heading = getByText('OnxGraph');
    expect(heading).toBeInTheDocument();
  });

  it('should display link to Onspring website', async () => {
    const { getByRole } = render(HeroGreeting);
    const link = getByRole('link', { name: 'Onspring' });
    expect(link).toHaveAttribute('href', 'https://onspring.com/');
  });
});

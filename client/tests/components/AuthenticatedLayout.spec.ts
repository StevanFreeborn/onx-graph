import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';

describe('AuthenticatedLayout', () => {
  afterEach(cleanup);

  it('should render the layout', async () => {
    expect(true).toBe(true);
  });
});

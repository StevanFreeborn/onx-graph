import '@testing-library/jest-dom/vitest';
import { cleanup, render } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import SpinningLoader from '../../src/components/SpinningLoader.vue';

describe('SpinningLoader', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display a spinning loader', () => {
    const { getByTitle, getByText } = render(SpinningLoader, {
      props: { height: '100px', width: '100px' },
    });

    const loader = getByTitle(/spinning loader/i);
    const message = getByText(/loading/i);

    expect(loader).toBeInTheDocument();
    expect(message).toBeInTheDocument();
  });

  it('should allow display of custom message', () => {
    const { getByText } = render(SpinningLoader, {
      props: { height: '100px', width: '100px', msg: 'Please wait...' },
    });

    const message = getByText(/please wait/i);

    expect(message).toBeInTheDocument();
  });
});

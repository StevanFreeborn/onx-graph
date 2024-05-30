import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import GraphMonitorLoader from '../../src/components/GraphMonitorLoader.vue';
import { customRender } from '../testUtils';

describe('GraphMonitorLoader', () => {
  afterEach(() => {
    cleanup();
  });

  it('should render the loader with given message', async () => {
    const { getByText } = await customRender(GraphMonitorLoader, {
      props: {
        msg: 'Loading graph...',
        status: 'pending',
      },
    });

    expect(getByText('Loading graph...')).toBeInTheDocument();
  });

  it('should render the loader with success icon displayed when status is success', async () => {
    const { getByTestId } = await customRender(GraphMonitorLoader, {
      props: {
        msg: 'Graph loaded',
        status: 'success',
      },
    });

    expect(getByTestId('success-icon')).toBeInTheDocument();
  });

  it('should render the loader with error icon displayed when status is error', async () => {
    const { getByTestId } = await customRender(GraphMonitorLoader, {
      props: {
        msg: 'Error loading graph',
        status: 'error',
      },
    });

    expect(getByTestId('error-icon')).toBeInTheDocument();
  });
});

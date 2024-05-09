import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import GraphHeading from '../../src/components/GraphHeading.vue';
import { GraphStatus } from '../../src/types';
import { customRender } from '../testUtils';

describe('GraphHeading', () => {
  afterEach(cleanup);

  it('should display the graph name', async () => {
    const { getByText } = await customRender(GraphHeading, {
      props: {
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const name = getByText(/test graph/i);

    expect(name).toBeInTheDocument();
  });

  it('should display graph status when built', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        name: 'Test Graph',
        status: GraphStatus.Built,
      },
    });

    const status = getByTitle(/built/i);

    expect(status).toBeInTheDocument();
  });

  it('should display graph status when building', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        name: 'Test Graph',
        status: GraphStatus.Building,
      },
    });

    const status = getByTitle(/building/i);

    expect(status).toBeInTheDocument();
  });

  it('should display graph status when not built', async () => {
    const { getByTitle } = await customRender(GraphHeading, {
      props: {
        name: 'Test Graph',
        status: GraphStatus.NotBuilt,
      },
    });

    const status = getByTitle(/not built/i);

    expect(status).toBeInTheDocument();
  });
});

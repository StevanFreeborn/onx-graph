import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import GraphGard from '../../src/components/GraphCard.vue';
import { customRender } from '../testUtils';

describe('GraphCard', () => {
  afterEach(cleanup);

  it('should throw error if no graph prop is provided', async () => {
    vi.spyOn(console, 'warn').mockImplementation(() => {});

    await expect(customRender(GraphGard)).rejects.toThrowError();
    expect(console.warn).toHaveBeenCalled();
  });

  it('should display a link to the graph', async () => {
    const { getByRole } = await customRender(GraphGard, {
      props: {
        graph: {
          id: 1,
          name: 'Test Graph',
          createdAt: '2021-01-01T00:00:00Z',
          updatedAt: '2021-01-01T00:00:00Z',
        },
      },
    });

    const link = getByRole('link', { name: /test graph/i });

    expect(link).toBeInTheDocument();
  });

  it("should display the graph's name", async () => {
    const { getByText } = await customRender(GraphGard, {
      props: {
        graph: {
          id: 1,
          name: 'Test Graph',
          createdAt: '2021-01-01T00:00:00Z',
          updatedAt: '2021-01-01T00:00:00Z',
        },
      },
    });

    const name = getByText(/test graph/i);

    expect(name).toBeInTheDocument();
  });

  it("should display the graph's created at date formatted", async () => {
    const createdDateString = '2021-01-01T00:00:00Z';
    const createdDateFormatted = new Date('2021-01-01T00:00:00Z').toLocaleString(undefined, {
      month: 'numeric',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      hour12: true,
    });

    const { getByText } = await customRender(GraphGard, {
      props: {
        graph: {
          id: 1,
          name: 'Test Graph',
          createdAt: createdDateString,
          updatedAt: '2021-01-01T00:00:00Z',
        },
      },
    });

    const createdAt = getByText(new RegExp(`created: ${createdDateFormatted}`, 'i'));

    expect(createdAt).toBeInTheDocument();
  });

  it("should display the graph's updated at date formatted", async () => {
    const updatedDateString = '2021-01-01T00:00:00Z';
    const updatedDateFormatted = new Date('2021-01-01T00:00:00Z').toLocaleString(undefined, {
      month: 'numeric',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      hour12: true,
    });

    const { getByText } = await customRender(GraphGard, {
      props: {
        graph: {
          id: 1,
          name: 'Test Graph',
          createdAt: '2021-01-01T00:00:00Z',
          updatedAt: updatedDateString,
        },
      },
    });

    const updatedAt = getByText(new RegExp(`updated: ${updatedDateFormatted}`, 'i'));

    expect(updatedAt).toBeInTheDocument();
  });
});

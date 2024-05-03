import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, render, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import DataPager from '../../src/components/DataPager.vue';

describe('DataPager', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  const mockFirstPage = {
    pageCount: 1,
    pageNumber: 1,
    totalPages: 3,
    totalCount: 3,
  };

  const mockSecondPage = {
    pageCount: 1,
    pageNumber: 2,
    totalPages: 3,
    totalCount: 3,
  };

  const mockLastPage = {
    pageCount: 1,
    pageNumber: 3,
    totalPages: 3,
    totalCount: 3,
  };

  it('should throw error if no page prop is provided', () => {
    vi.spyOn(console, 'warn').mockImplementation(() => {});

    expect(() => render(DataPager)).toThrowError();

    expect(console.warn).toHaveBeenCalled();
  });

  it('should display a previous button', () => {
    const { getByRole } = render(DataPager, {
      props: { page: mockFirstPage },
    });

    const previousButton = getByRole('button', { name: /previous/i });

    expect(previousButton).toBeInTheDocument();
  });

  it('should display a next button', () => {
    const { getByRole } = render(DataPager, {
      props: { page: mockLastPage },
    });

    const nextButton = getByRole('button', { name: /next/i });

    expect(nextButton).toBeInTheDocument();
  });

  it('should display current page and total page number', () => {
    const { getByText } = render(DataPager, {
      props: { page: mockFirstPage },
    });

    const currentPage = getByText(
      new RegExp(`${mockFirstPage.pageNumber} of ${mockFirstPage.totalPages}`, 'i')
    );

    expect(currentPage).toBeInTheDocument();
  });

  it('should disable previous button if current page is 1', () => {
    const { getByRole } = render(DataPager, {
      props: { page: mockFirstPage },
    });

    const previousButton = getByRole('button', { name: /previous/i });

    expect(previousButton).toBeDisabled();
  });

  it('should disable next button if current page is the last page', () => {
    const { getByRole } = render(DataPager, {
      props: { page: mockLastPage },
    });

    const nextButton = getByRole('button', { name: /next/i });

    expect(nextButton).toBeDisabled();
  });

  it('should display previous and next buttons as enabled if current page is not the first or last page', async () => {
    const { getByRole } = render(DataPager, {
      props: { page: mockSecondPage },
    });

    const previousButton = getByRole('button', { name: /previous/i });
    const nextButton = getByRole('button', { name: /next/i });

    await waitFor(() => {
      expect(previousButton).toBeEnabled();
      expect(nextButton).toBeEnabled();
    });
  });

  it('should emit previous event when previous button is clicked', async () => {
    const { emitted, getByRole } = render(DataPager, {
      props: { page: mockSecondPage },
    });

    const previousButton = getByRole('button', { name: /previous/i });

    await fireEvent.click(previousButton);

    expect(emitted().previous).toBeTruthy();
  });

  it('should emit next event when next button is clicked', () => {
    const { emitted, getByRole } = render(DataPager, {
      props: { page: mockFirstPage },
    });

    const nextButton = getByRole('button', { name: /next/i });

    fireEvent.click(nextButton);

    expect(emitted().next).toBeTruthy();
  });
});

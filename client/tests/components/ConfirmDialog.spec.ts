import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent } from '@testing-library/vue';
import { afterEach, describe, expect, it } from 'vitest';
import ConfirmDialog from '../../src/components/ConfirmDialog.vue';
import { customRender } from '../testUtils';

describe('ConfirmDialog', () => {
  afterEach(cleanup);

  it('should display itself when show is true', async () => {
    const { getByRole } = await customRender(ConfirmDialog, {
      props: {
        show: true,
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
      },
    });

    const dialog = getByRole('alertdialog');

    expect(dialog).toBeInTheDocument();
  });

  it('should not display itself when show is false', async () => {
    const { queryByRole } = await customRender(ConfirmDialog, {
      props: {
        show: false,
        confirmButtonText: 'Confirm',
        cancelButtonText: 'Cancel',
      },
    });

    const dialog = queryByRole('alertdialog');

    expect(dialog).not.toBeInTheDocument();
  });

  it('should display buttons with the given confirm and cancel text', async () => {
    const { getByRole } = await customRender(ConfirmDialog, {
      props: {
        show: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
      },
    });

    const confirmButton = getByRole('button', { name: 'Yes' });
    const cancelButton = getByRole('button', { name: 'No' });

    expect(confirmButton).toBeInTheDocument();
    expect(cancelButton).toBeInTheDocument();
  });

  it('should emit confirm event when confirm button is clicked', async () => {
    const { getByRole, emitted } = await customRender(ConfirmDialog, {
      props: {
        show: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
      },
    });

    const confirmButton = getByRole('button', { name: 'Yes' });

    await fireEvent.click(confirmButton);

    expect(emitted().confirm).toHaveLength(1);
  });

  it('should emit cancel event when cancel button is clicked', async () => {
    const { getByRole, emitted } = await customRender(ConfirmDialog, {
      props: {
        show: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
      },
    });

    const cancelButton = getByRole('button', { name: 'No' });

    await fireEvent.click(cancelButton);

    expect(emitted().cancel).toHaveLength(1);
  });

  it('should emit cancel event when overlay is clicked', async () => {
    const { getByRole, emitted } = await customRender(ConfirmDialog, {
      props: {
        show: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No',
      },
    });

    const overlay = getByRole('alertdialog');

    await fireEvent.click(overlay);

    expect(emitted().cancel).toHaveLength(1);
  });
});

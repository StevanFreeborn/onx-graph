import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import ResendVerifyEmailForm from '../../src/components/ResendVerifyEmailForm.vue';
import { customRender } from '../testUtils';

describe('ResendVerifyEmailForm', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display a form to to resend the verification email', async () => {
    const { getByRole } = await customRender(ResendVerifyEmailForm);
    const resendForm = getByRole('form', { name: /resend verification email form/i });
    expect(resendForm).toBeInTheDocument();
  });

  it('should display an email input', async () => {
    const { getByRole } = await customRender(ResendVerifyEmailForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    expect(emailInput).toBeInTheDocument();
  });

  describe('email input', () => {
    it('should be required', async () => {
      const { getByRole } = await customRender(ResendVerifyEmailForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toBeRequired();
    });

    it('should be of type email', async () => {
      const { getByRole } = await customRender(ResendVerifyEmailForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toHaveAttribute('type', 'email');
    });
  });

  it('should display a button to resend the verification email', async () => {
    const { getByRole } = await customRender(ResendVerifyEmailForm);
    const resendButton = getByRole('button', { name: /resend/i });
    expect(resendButton).toBeInTheDocument();
  });

  it('should display error message if submitted with no email', async () => {
    const { getByRole, getByText } = await customRender(ResendVerifyEmailForm);
    const resendButton = getByRole('button', { name: /resend/i });

    await fireEvent.click(resendButton);

    const emailError = getByText(/email is required/i);
    expect(emailError).toBeInTheDocument();
  });

  it('should hide error message when email is entered', async () => {
    const { getByRole, getByText } = await customRender(ResendVerifyEmailForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const resendButton = getByRole('button', { name: /resend/i });

    await fireEvent.click(resendButton);
    const emailError = getByText(/email is required/i);
    expect(emailError).toBeInTheDocument();

    await fireEvent.update(emailInput, 'test@test.com');
    expect(emailError).not.toBeInTheDocument();
  });

  it('should display error message if submitted with invalid email', async () => {
    const { getByRole, getByText } = await customRender(ResendVerifyEmailForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const resendButton = getByRole('button', { name: /resend/i });

    await fireEvent.update(emailInput, 'invalid-email');
    await fireEvent.click(resendButton);

    const emailError = getByText(/enter a valid email address/i);

    expect(emailError).toBeInTheDocument();
  });
});

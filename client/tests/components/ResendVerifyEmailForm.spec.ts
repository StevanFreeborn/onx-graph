import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import ResendVerifyEmailForm from '../../src/components/ResendVerifyEmailForm.vue';
import { AuthServiceFactoryKey } from '../../src/services/authService';
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

  describe('resend button', () => {
    it('should be of type submit', async () => {
      const { getByRole } = await customRender(ResendVerifyEmailForm);
      const resendButton = getByRole('button', { name: /resend/i });
      expect(resendButton).toHaveAttribute('type', 'submit');
    });

    it('should have a label of "Resend"', async () => {
      const { getByRole } = await customRender(ResendVerifyEmailForm);
      const resendButton = getByRole('button', { name: /resend/i });
      expect(resendButton).toHaveTextContent('Resend');
    });

    it('should be enabled when form is not submitting', async () => {
      const { getByRole } = await customRender(ResendVerifyEmailForm);
      const resendButton = getByRole('button', { name: /resend/i });
      expect(resendButton).toBeEnabled();
    });

    it('should be disabled when form is submitting', async () => {
      const mockAuthService = {
        resendVerificationEmail: vi.fn(),
      };

      const fakeRequest = new Promise(resolve =>
        setTimeout(() => {
          resolve({ err: true, val: ["That didn't work"] });
        }, 100)
      );

      mockAuthService.resendVerificationEmail.mockResolvedValueOnce(fakeRequest);

      const { getByRole } = await customRender(ResendVerifyEmailForm, {
        global: {
          provide: {
            [AuthServiceFactoryKey as symbol]: {
              create: () => mockAuthService,
            },
          },
        },
      });

      const emailInput = getByRole('textbox', { name: /email/i });
      const resendButton = getByRole('button', { name: /resend/i });

      await fireEvent.update(emailInput, 'test@test.com');
      await fireEvent.click(resendButton);

      expect(resendButton).toBeDisabled();

      await waitFor(() => {
        expect(resendButton).toBeEnabled();
      });
    });
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

  it('should display then hide error message when sending fails', async () => {
    const mockAuthService = {
      resendVerificationEmail: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: true, val: ["That didn't work"] });
      }, 100)
    );

    mockAuthService.resendVerificationEmail.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByText } = await customRender(ResendVerifyEmailForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const emailInput = getByRole('textbox', { name: /email/i });
    const resendButton = getByRole('button', { name: /resend/i });

    await fireEvent.update(emailInput, 'test@test.com');
    await fireEvent.click(resendButton);

    const sendingMessage = getByText(/sending/i);
    expect(sendingMessage).toBeInTheDocument();

    await waitFor(() => {
      expect(sendingMessage).not.toBeInTheDocument();
    });

    const errorMessage = getByText(/unable to send email/i);
    expect(errorMessage).toBeInTheDocument();

    await waitFor(() => {
      expect(errorMessage).not.toBeInTheDocument();
    });
  });

  it('should display then hide success message when sending succeeds', async () => {
    const mockAuthService = {
      resendVerificationEmail: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: false, val: true });
      }, 100)
    );

    mockAuthService.resendVerificationEmail.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByText } = await customRender(ResendVerifyEmailForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const emailInput = getByRole('textbox', { name: /email/i });
    const resendButton = getByRole('button', { name: /resend/i });

    await fireEvent.update(emailInput, 'test@test.com');
    await fireEvent.click(resendButton);

    const sendingMessage = getByText(/sending/i);
    expect(sendingMessage).toBeInTheDocument();

    await waitFor(() => {
      expect(sendingMessage).not.toBeInTheDocument();
    });

    const successMessage = getByText(/email sent/i);
    expect(successMessage).toBeInTheDocument();

    await waitFor(() => {
      expect(successMessage).not.toBeInTheDocument();
    });
  });
});

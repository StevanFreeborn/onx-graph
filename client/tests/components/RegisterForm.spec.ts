import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import RegisterForm from '../../src/components/RegisterForm.vue';
import { customRender } from '../testUtils';

describe('RegisterForm', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display a register form', async () => {
    const { getByRole } = await customRender(RegisterForm);
    const registerForm = getByRole('form', { name: 'register form' });
    expect(registerForm).toBeInTheDocument();
  });

  it('should display an email input', async () => {
    const { getByRole } = await customRender(RegisterForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    expect(emailInput).toBeInTheDocument();
  });

  describe('email input', () => {
    it('should be required', async () => {
      const { getByRole } = await customRender(RegisterForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toBeRequired();
    });

    it('should be of type email', async () => {
      const { getByRole } = await customRender(RegisterForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toHaveAttribute('type', 'email');
    });
  });

  it('should display a password input', async () => {
    const { getByLabelText } = await customRender(RegisterForm);
    const passwordInput = getByLabelText('Password', { exact: true });
    expect(passwordInput).toBeInTheDocument();
  });

  describe('password input', () => {
    it('should be required', async () => {
      const { getByLabelText } = await customRender(RegisterForm);
      const passwordInput = getByLabelText('Password', { exact: true });
      expect(passwordInput).toBeRequired();
    });

    it('should be of type password', async () => {
      const { getByLabelText } = await customRender(RegisterForm);
      const passwordInput = getByLabelText('Password', { exact: true });
      expect(passwordInput).toHaveAttribute('type', 'password');
    });
  });

  it('should display a password confirmation input', async () => {
    const { getByLabelText } = await customRender(RegisterForm);
    const passwordConfirmationInput = getByLabelText(/confirm password/i);
    expect(passwordConfirmationInput).toBeInTheDocument();
  });

  describe('password confirmation input', () => {
    it('should be required', async () => {
      const { getByLabelText } = await customRender(RegisterForm);
      const passwordConfirmationInput = getByLabelText(/confirm password/i);
      expect(passwordConfirmationInput).toBeRequired();
    });

    it('should be of type password', async () => {
      const { getByLabelText } = await customRender(RegisterForm);
      const passwordConfirmationInput = getByLabelText(/confirm password/i);
      expect(passwordConfirmationInput).toHaveAttribute('type', 'password');
    });
  });

  it('should display a register button', async () => {
    const { getByRole } = await customRender(RegisterForm);
    const registerButton = getByRole('button', { name: /register/i });
    expect(registerButton).toBeInTheDocument();
  });

  it('should display error message if submitted with no email', async () => {
    const { getByRole, getByText } = await customRender(RegisterForm);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);

    const emailError = getByText(/email is required/i);
    expect(emailError).toBeInTheDocument();
  });

  it('should hide error message when email is entered', async () => {
    const { getByRole, getByText } = await customRender(RegisterForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);
    const emailError = getByText(/email is required/i);
    expect(emailError).toBeInTheDocument();

    await fireEvent.update(emailInput, 'test@test.com');
    expect(emailError).not.toBeInTheDocument();
  });

  it('should display error message if submitted with invalid email', async () => {
    const { getByRole, getByLabelText, getByText } = await customRender(RegisterForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText('Password', { exact: true });
    const confirmInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.update(emailInput, 'invalid-email');
    await fireEvent.update(passwordInput, 'password');
    await fireEvent.update(confirmInput, 'password');
    await fireEvent.click(registerButton);

    const emailError = getByText(/enter a valid email address/i);

    expect(emailError).toBeInTheDocument();
  });

  it('should display error message if submitted with no password', async () => {
    const { getByRole, getByText } = await customRender(RegisterForm);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);

    const passwordError = getByText(/^password is required\.$/i);
    expect(passwordError).toBeInTheDocument();
  });

  it('should hide error message when password is entered', async () => {
    const { getByRole, getByLabelText, getByText } = await customRender(RegisterForm);
    const passwordInput = getByLabelText('Password', { exact: true });
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);
    const passwordError = getByText(/^password is required\.$/i);
    expect(passwordError).toBeInTheDocument();

    await fireEvent.update(passwordInput, 'password');
    expect(passwordError).not.toBeInTheDocument();
  });

  it('should display error message if submitted with no password confirmation', async () => {
    const { getByRole, getByText } = await customRender(RegisterForm);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);

    const passwordConfirmationError = getByText(/confirm password is required/i);
    expect(passwordConfirmationError).toBeInTheDocument();
  });

  it('should hide error message when password confirmation is entered', async () => {
    const { getByRole, getByLabelText, getByText } = await customRender(RegisterForm);
    const passwordConfirmationInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.click(registerButton);
    const passwordConfirmationError = getByText(/confirm password is required/i);
    expect(passwordConfirmationError).toBeInTheDocument();

    await fireEvent.update(passwordConfirmationInput, 'password');
    expect(passwordConfirmationError).not.toBeInTheDocument();
  });

  it('should display error message if password and password confirmation do not match', async () => {
    const { getByRole, getByLabelText, getByText } = await customRender(RegisterForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText('Password', { exact: true });
    const confirmInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.update(emailInput, 'test@test.com');
    await fireEvent.update(passwordInput, 'password');
    await fireEvent.update(confirmInput, 'password1');
    await fireEvent.click(registerButton);

    const passwordConfirmationError = getByText(/passwords do not match/i);

    expect(passwordConfirmationError).toBeInTheDocument();
  });

  it('should display error message if form is submitted with invalid credentials', async () => {
    expect(false).toBe(true);
  });

  it('should redirect to /login if form is submitted with valid credentials', async () => {
    expect(false).toBe(true);
  });
});

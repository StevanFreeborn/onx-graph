import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import RegisterForm from '../../src/components/RegisterForm.vue';
import { AuthServiceFactoryKey } from '../../src/services/authService';
import { customRender } from '../testUtils';

vi.mock('vue-router', async importOriginal => {
  const actual = await importOriginal<typeof import('vue-router')>();
  const mock = vi.fn();
  return {
    ...actual,
    useRouter: () => ({ push: mock }),
  };
});

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

  it('should display error message if form is submitted with invalid information', async () => {
    const mockAuthService = {
      register: vi.fn(),
    };

    mockAuthService.register.mockReturnValueOnce({
      err: true,
      val: [new Error('Email already exists')],
    });

    const { getByRole, getByLabelText, getByText } = await customRender(RegisterForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const newUserInfo = {
      email: 'test@test.com',
      password: 'password',
    };

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText('Password', { exact: true });
    const confirmInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.update(emailInput, newUserInfo.email);
    await fireEvent.update(passwordInput, newUserInfo.password);
    await fireEvent.update(confirmInput, newUserInfo.password);
    await fireEvent.click(registerButton);

    const errorMessage = getByText(/email already exists/i);
    expect(errorMessage).toBeInTheDocument();

    expect(mockAuthService.register).toHaveBeenCalledTimes(1);
    expect(mockAuthService.register).toHaveBeenCalledWith(newUserInfo.email, newUserInfo.password);
  });

  it('should disable the register button when the form is submitting', async () => {
    const mockAuthService = {
      register: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: false, val: ['User already exists'] });
      }, 100)
    );

    mockAuthService.register.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByLabelText } = await customRender(RegisterForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText('Password', { exact: true });
    const confirmInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    expect(registerButton).toBeEnabled();

    await fireEvent.update(emailInput, 'test@test.com');
    await fireEvent.update(passwordInput, 'password');
    await fireEvent.update(confirmInput, 'password');
    await fireEvent.click(registerButton);

    expect(registerButton).toBeDisabled();

    await waitFor(() => {
      expect(registerButton).toBeEnabled();
    });
  });

  it('should redirect to /login if form is submitted with valid credentials', async () => {
    const mockAuthService = {
      register: vi.fn(),
    };

    const successResponse = {
      err: false,
      val: {
        id: 'new-user-id',
      },
    };

    mockAuthService.register.mockReturnValueOnce(successResponse);

    const { getByRole, getByLabelText } = await customRender(RegisterForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const newUserInfo = {
      email: 'test@test.com',
      password: 'password',
    };

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText('Password', { exact: true });
    const confirmPasswordInput = getByLabelText(/confirm password/i);
    const registerButton = getByRole('button', { name: /register/i });

    await fireEvent.update(emailInput, newUserInfo.email);
    await fireEvent.update(passwordInput, newUserInfo.password);
    await fireEvent.update(confirmPasswordInput, newUserInfo.password);
    await fireEvent.click(registerButton);

    expect(mockAuthService.register).toHaveBeenCalledTimes(1);
    expect(mockAuthService.register).toHaveBeenCalledWith(newUserInfo.email, newUserInfo.password);

    const { push: pushMock } = useRouter();

    expect(pushMock).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith('/login');
  });
});

import '@testing-library/jest-dom/vitest';
import { cleanup, fireEvent, waitFor } from '@testing-library/vue';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import LoginForm from '../../src/components/LoginForm.vue';
import { customRender } from '../testUtils';
import { AuthServiceFactoryKey } from './../../src/services/authService';
import { useUserStore } from './../../src/stores/userStore';

vi.mock('vue-router', async importOriginal => {
  const actual = await importOriginal<typeof import('vue-router')>();
  const mock = vi.fn();
  return {
    ...actual,
    useRouter: () => ({ push: mock }),
  };
});

describe('LoginForm', () => {
  afterEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('should display a login form', async () => {
    const { getByRole } = await customRender(LoginForm);
    const loginForm = getByRole('form', { name: 'login form' });
    expect(loginForm).toBeInTheDocument();
  });

  it('should display an email input', async () => {
    const { getByRole } = await customRender(LoginForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    expect(emailInput).toBeInTheDocument();
  });

  describe('email input', () => {
    it('should be required', async () => {
      const { getByRole } = await customRender(LoginForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toBeRequired();
    });

    it('should have a type of email', async () => {
      const { getByRole } = await customRender(LoginForm);
      const emailInput = getByRole('textbox', { name: /email/i });
      expect(emailInput).toHaveAttribute('type', 'email');
    });
  });

  it('should display a password input', async () => {
    const { getByLabelText } = await customRender(LoginForm);
    const passwordInput = getByLabelText(/password/i);
    expect(passwordInput).toBeInTheDocument();
  });

  describe('password input', () => {
    it('should be required', async () => {
      const { getByLabelText } = await customRender(LoginForm);
      const passwordInput = getByLabelText(/password/i);
      expect(passwordInput).toBeRequired();
    });

    it('should have a type of password', async () => {
      const { getByLabelText } = await customRender(LoginForm);
      const passwordInput = getByLabelText(/password/i);
      expect(passwordInput).toHaveAttribute('type', 'password');
    });
  });

  it('should display a login button', async () => {
    const { getByRole } = await customRender(LoginForm);
    const loginButton = getByRole('button', { name: /login/i });
    expect(loginButton).toBeInTheDocument();
  });

  it('should display error message if form is submitted with no email', async () => {
    const { getByRole, getByText } = await customRender(LoginForm);
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.click(loginButton);

    const errorMessage = getByText(/email is required/i);
    expect(errorMessage).toBeInTheDocument();
  });

  it('should display error message if form is submitted with invalid email', async () => {
    const { getByRole, getByText } = await customRender(LoginForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.update(emailInput, 'test');

    await fireEvent.click(loginButton);

    const errorMessage = getByText(/enter a valid email address/i);
    expect(errorMessage).toBeInTheDocument();
  });

  it('should display error message if form is submitted with no password', async () => {
    const { getByRole, getByText } = await customRender(LoginForm);
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.click(loginButton);

    const errorMessage = getByText(/password is required/i);
    expect(errorMessage).toBeInTheDocument();
  });

  it('should hide error message when email is entered', async () => {
    const { getByRole, queryByText } = await customRender(LoginForm);
    const emailInput = getByRole('textbox', { name: /email/i });
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.click(loginButton);

    expect(queryByText(/email is required/i)).toBeInTheDocument();

    await fireEvent.update(emailInput, 'test@test.com');

    expect(queryByText(/email is required/i)).not.toBeInTheDocument();
  });

  it('should hide error message when password is entered', async () => {
    const { getByRole, getByLabelText, queryByText } = await customRender(LoginForm);
    const passwordInput = getByLabelText(/password/i);
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.click(loginButton);

    expect(queryByText(/password is required/i)).toBeInTheDocument();

    await fireEvent.update(passwordInput, 'password');

    expect(queryByText(/password is required/i)).not.toBeInTheDocument();
  });

  it('should display error message if form is submitted with invalid credentials', async () => {
    const mockAuthService = {
      login: vi.fn(),
    };

    mockAuthService.login.mockReturnValueOnce({
      err: true,
      val: [new Error('Invalid credentials')],
    });

    const { getByRole, getByLabelText, getByText } = await customRender(LoginForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const invalidCredentials = {
      email: 'test@test.com',
      password: 'password',
    };

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText(/password/i);
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.update(emailInput, invalidCredentials.email);
    await fireEvent.update(passwordInput, invalidCredentials.password);
    await fireEvent.click(loginButton);

    const errorMessage = getByText(/invalid credentials/i);
    expect(errorMessage).toBeInTheDocument();

    expect(mockAuthService.login).toHaveBeenCalledTimes(1);
    expect(mockAuthService.login).toHaveBeenCalledWith(
      invalidCredentials.email,
      invalidCredentials.password
    );
  });

  it('should disable the login button when the form is submitting', async () => {
    const mockAuthService = {
      login: vi.fn(),
    };

    const fakeRequest = new Promise(resolve =>
      setTimeout(() => {
        resolve({ err: false, val: ['Invalid Credentials'] });
      }, 100)
    );

    mockAuthService.login.mockResolvedValueOnce(fakeRequest);

    const { getByRole, getByLabelText } = await customRender(LoginForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText(/password/i);
    const loginButton = getByRole('button', { name: /login/i });

    expect(loginButton).toBeEnabled();

    await fireEvent.update(emailInput, 'test@test.com');
    await fireEvent.update(passwordInput, 'password');
    await fireEvent.click(loginButton);

    expect(loginButton).toBeDisabled();

    await waitFor(() => {
      expect(loginButton).toBeEnabled();
    });
  });

  it('should log user in when form is submitted with valid credentials', async () => {
    const mockAuthService = {
      login: vi.fn(),
    };

    const successResponse = {
      err: false,
      val: {
        accessToken: 'token',
      },
    };

    mockAuthService.login.mockReturnValueOnce(successResponse);

    const { getByRole, getByLabelText } = await customRender(LoginForm, {
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    const userStore = useUserStore();

    const validCredentials = {
      email: 'test@test.com',
      password: 'password',
    };

    const emailInput = getByRole('textbox', { name: /email/i });
    const passwordInput = getByLabelText(/password/i);
    const loginButton = getByRole('button', { name: /login/i });

    await fireEvent.update(emailInput, validCredentials.email);
    await fireEvent.update(passwordInput, validCredentials.password);
    await fireEvent.click(loginButton);

    expect(mockAuthService.login).toHaveBeenCalledTimes(1);
    expect(mockAuthService.login).toHaveBeenCalledWith(
      validCredentials.email,
      validCredentials.password
    );

    expect(userStore.logUserIn).toHaveBeenCalledTimes(1);
    expect(userStore.logUserIn).toHaveBeenCalledWith(successResponse.val.accessToken);

    const { push: pushMock } = useRouter();

    expect(pushMock).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith('/graphs');
  });
});

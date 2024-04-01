import '@testing-library/jest-dom/vitest';
import { cleanup, waitFor } from '@testing-library/vue';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { useRouter } from 'vue-router';
import AccountVerifier from '../../src/components/AccountVerifier.vue';
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

describe('AccountVerifier', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    cleanup();
    vi.useRealTimers();
    vi.resetAllMocks();
  });

  it('should display starting verification message', async () => {
    const { getByText } = await customRender(AccountVerifier, {
      props: {
        token: 'test',
      },
    });

    expect(getByText(/starting verification process/i)).toBeInTheDocument();
  });

  it('should display retrieving token message after 1 second', async () => {
    const { getByText } = await customRender(AccountVerifier, {
      props: {
        token: 'test',
      },
    });

    await vi.advanceTimersByTimeAsync(1000);

    expect(getByText(/retrieving token/i)).toBeInTheDocument();
  });

  it('should display unverified message after 2 seconds if no token is provided', async () => {
    const { getByText, getByRole } = await customRender(AccountVerifier);

    vi.advanceTimersByTime(2000);

    await waitFor(() => {
      expect(getByText(/we were unable to verify your account/i)).toBeInTheDocument();
      expect(getByRole('form', { name: 'resend verification email form' })).toBeInTheDocument();
    });
  });

  it('should display unverified message after 2 seconds if token is not a string', async () => {
    const { getByText, getByRole } = await customRender(AccountVerifier, {
      props: {
        token: ['test', 'test'],
      },
    });

    vi.advanceTimersByTime(2000);

    await waitFor(() => {
      expect(getByText(/we were unable to verify your account/i)).toBeInTheDocument();
      expect(getByRole('form', { name: 'resend verification email form' })).toBeInTheDocument();
    });
  });

  it('should display unverified message after 2 seconds if token is an empty string', async () => {
    const { getByText, getByRole } = await customRender(AccountVerifier, {
      props: {
        token: '',
      },
    });

    await vi.advanceTimersByTimeAsync(2000);

    expect(getByText(/we were unable to verify your account/i)).toBeInTheDocument();
    expect(getByRole('form', { name: 'resend verification email form' })).toBeInTheDocument();
  });

  it('should display verifying message after 2 seconds if token is provided', async () => {
    const mockAuthService = {
      verifyAccount: vi.fn(),
    };

    mockAuthService.verifyAccount.mockResolvedValue({ err: false });

    const token = 'test';

    const { getByText } = await customRender(AccountVerifier, {
      props: {
        token: token,
      },
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    await vi.advanceTimersByTimeAsync(2000);

    expect(getByText(/verifying token/i)).toBeInTheDocument();

    expect(mockAuthService.verifyAccount).toHaveBeenCalledTimes(1);
    expect(mockAuthService.verifyAccount).toHaveBeenCalledWith(token);
  });

  it('should display unverified message with error messages after 3 seconds if verification fails', async () => {
    const mockAuthService = {
      verifyAccount: vi.fn(),
    };

    const error = new Error('It broke');

    mockAuthService.verifyAccount.mockResolvedValue({ err: true, val: [error] });

    const token = 'test';

    const { getByText } = await customRender(AccountVerifier, {
      props: {
        token: token,
      },
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    await vi.advanceTimersByTimeAsync(3000);

    expect(getByText(/we were unable to verify your account/i)).toBeInTheDocument();
    expect(getByText(new RegExp(error.message, 'i'))).toBeInTheDocument();

    expect(mockAuthService.verifyAccount).toHaveBeenCalledTimes(1);
    expect(mockAuthService.verifyAccount).toHaveBeenCalledWith(token);
  });

  it('should display verified message after 3 seconds if verification succeeds', async () => {
    const mockAuthService = {
      verifyAccount: vi.fn(),
    };

    mockAuthService.verifyAccount.mockResolvedValue({ err: false });

    const token = 'test';

    const { getByText } = await customRender(AccountVerifier, {
      props: {
        token: token,
      },
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    await vi.advanceTimersByTimeAsync(3000);

    expect(getByText(/successfully verified account/i)).toBeInTheDocument();

    expect(mockAuthService.verifyAccount).toHaveBeenCalledTimes(1);
    expect(mockAuthService.verifyAccount).toHaveBeenCalledWith(token);
  });

  it('should redirect to login page after 4 seconds if verification succeeds', async () => {
    const mockAuthService = {
      verifyAccount: vi.fn(),
    };

    mockAuthService.verifyAccount.mockResolvedValue({ err: false });

    const token = 'test';

    await customRender(AccountVerifier, {
      props: {
        token: token,
      },
      global: {
        provide: {
          [AuthServiceFactoryKey as symbol]: {
            create: () => mockAuthService,
          },
        },
      },
    });

    await vi.advanceTimersByTimeAsync(4000);

    const { push: pushMock } = useRouter();

    expect(pushMock).toHaveBeenCalledTimes(1);
    expect(pushMock).toHaveBeenCalledWith({ name: 'login' });
  });
});

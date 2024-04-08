import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthService, UserNotVerifiedError } from '../../src/services/authService';
import { mockClient } from '../testUtils';

describe('AuthService', () => {
  let authService: AuthService;

  beforeEach(() => {
    authService = new AuthService(mockClient);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should have a constructor that returns a new instance', () => {
    expect(authService).toBeInstanceOf(AuthService);
  });

  it('should have a logout method', () => {
    expect(authService.logout).toBeInstanceOf(Function);
  });

  it('should have a refreshToken method', () => {
    expect(authService.refreshToken).toBeInstanceOf(Function);
  });

  it('should have a login method', () => {
    expect(authService.login).toBeInstanceOf(Function);
  });

  it('should have a register method', () => {
    expect(authService.register).toBeInstanceOf(Function);
  });

  it('should have a resendVerificationEmail method', () => {
    expect(authService.resendVerificationEmail).toBeInstanceOf(Function);
  });

  it('should have a verifyAccount method', () => {
    expect(authService.verifyAccount).toBeInstanceOf(Function);
  });

  describe('logout', () => {
    it('should return error if logout fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.logout();

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return error if error occurs while logging out', async () => {
      console.error = vi.fn();
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.logout();

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(console.error).toHaveBeenCalled();
    });

    it('should return true if logout is successful', async () => {
      mockClient.post.mockReturnValue({ ok: true });

      const result = await authService.logout();

      expect(result.ok).toBe(true);
      expect(result.val).toBe(true);
    });
  });

  describe('refreshToken', () => {
    it('should return error if refresh token is not valid', async () => {
      mockClient.post.mockReturnValue({ status: 401 });

      const result = await authService.refreshToken();

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return error if refreshing token fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.refreshToken();

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return error if error occurs while refreshing token', async () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.refreshToken();

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(consoleSpy).toHaveBeenCalled();
    });

    it('should return body if refreshing token is successful', async () => {
      const body = { token: 'token' };
      mockClient.post.mockReturnValue({ ok: true, json: () => body });

      const result = await authService.refreshToken();

      expect(result.ok).toBe(true);
      expect(result.val).toBe(body);
    });
  });

  describe('login', () => {
    it('should return error if login fails due to validation', async () => {
      const body = {
        errors: { Email: ['Email is not valid'], Password: ['Password is not valid'] },
      };

      mockClient.post.mockReturnValue({
        status: 400,
        json: () => body,
      });

      const result = await authService.login('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error), expect.any(Error)]);
    });

    it('should return error if login fails due to invalid email/password combination', async () => {
      mockClient.post.mockReturnValue({ status: 401 });

      const result = await authService.login('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return error if error occurs while logging in', async () => {
      console.error = vi.fn();
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.login('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(console.error).toHaveBeenCalled();
    });

    it('should return error if login fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.login('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return body if login is successful', async () => {
      const body = { token: 'token' };
      mockClient.post.mockReturnValue({ ok: true, json: () => body });

      const result = await authService.login('email', 'password');

      expect(result.ok).toBe(true);
      expect(result.val).toBe(body);
    });

    it('should return error if user is not verified', async () => {
      mockClient.post.mockReturnValue({ status: 403 });

      const result = await authService.login('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(UserNotVerifiedError)]);
    });
  });

  describe('register', () => {
    it('should return error if register fails due to validation', async () => {
      const body = {
        errors: { Email: ['Email is not valid'], Password: ['Password is not valid'] },
      };

      mockClient.post.mockReturnValue({
        status: 400,
        json: () => body,
      });

      const result = await authService.register('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error), expect.any(Error)]);
    });

    it('should return error if error occurs while registering', async () => {
      console.error = vi.fn();
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.register('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(console.error).toHaveBeenCalled();
    });

    for (const status of [409]) {
      it(`should return error if registering fails with status ${status}`, async () => {
        mockClient.post.mockReturnValue({ status });

        const result = await authService.register('email', 'password');

        expect(result.err).toBe(true);
        expect(result.val).toEqual([expect.any(Error)]);
      });
    }

    it('should return error if register fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.register('email', 'password');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return body if register is successful', async () => {
      const body = { id: 'id' };
      mockClient.post.mockReturnValue({ ok: true, json: () => body });

      const result = await authService.register('email', 'password');

      expect(result.ok).toBe(true);
      expect(result.val).toBe(body);
    });
  });

  describe('resendVerificationEmail', () => {
    it('should return error if error occurs while resending verification email', async () => {
      console.error = vi.fn();
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.resendVerificationEmail('email');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(console.error).toHaveBeenCalled();
    });

    it('should return error if resending verification email fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.resendVerificationEmail('email');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    it('should return error if validation fails while resending verification email', async () => {
      const body = {
        errors: { Email: ['Email is not valid'] },
      };

      mockClient.post.mockReturnValue({
        status: 400,
        json: () => body,
      });

      const result = await authService.resendVerificationEmail('email');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Email is not valid')]);
    });

    for (const status of [404, 409]) {
      it(`should return error if resending verification email fails with status ${status}`, async () => {
        mockClient.post.mockReturnValue({ status });

        const result = await authService.resendVerificationEmail('email');

        expect(result.err).toBe(true);
        expect(result.val).toEqual([expect.any(Error)]);
      });
    }

    it('should return true if resending verification email is successful', async () => {
      mockClient.post.mockReturnValue({ ok: true });

      const result = await authService.resendVerificationEmail('email');

      expect(result.ok).toBe(true);
      expect(result.val).toBe(true);
    });
  });

  describe('verifyAccount', () => {
    it('should return error if error occurs while verifying account', async () => {
      console.error = vi.fn();
      mockClient.post.mockRejectedValue(new Error('error'));

      const result = await authService.verifyAccount('token');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
      expect(console.error).toHaveBeenCalled();
    });

    it('should return error if verifying account fails', async () => {
      mockClient.post.mockReturnValue({ ok: false });

      const result = await authService.verifyAccount('token');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([expect.any(Error)]);
    });

    for (const status of [400, 404, 409]) {
      it(`should return error if verifying account fails with status ${status}`, async () => {
        mockClient.post.mockReturnValue({ status });

        const result = await authService.verifyAccount('token');

        expect(result.err).toBe(true);
        expect(result.val).toEqual([expect.any(Error)]);
      });
    }

    it('should return true if verifying account is successful', async () => {
      mockClient.post.mockReturnValue({ ok: true });

      const result = await authService.verifyAccount('token');

      expect(result.ok).toBe(true);
      expect(result.val).toBe(true);
    });
  });
});

import { Err, Ok, Result } from 'ts-results';
import { type InjectionKey } from 'vue';
import { ClientRequestWithBody, type IClient } from './client';

type AuthServiceFactoryKeyType = InjectionKey<IAuthServiceFactory>;

export const AuthServiceFactoryKey: AuthServiceFactoryKeyType = Symbol('AuthServiceFactory');

export interface IAuthServiceFactory {
  create: (client: IClient) => IAuthService;
}

export class AuthServiceFactory implements IAuthServiceFactory {
  create(client: IClient): IAuthService {
    return new AuthService(client);
  }
}

export interface IAuthService {
  login: (email: string, password: string) => Promise<Result<LoginResponse, Error[]>>;
  register: (email: string, password: string) => Promise<Result<RegisterResponse, Error[]>>;
  refreshToken: () => Promise<Result<LoginResponse, Error[]>>;
  logout: () => Promise<Result<boolean, Error[]>>;
  resendVerificationEmail: (email: string) => Promise<Result<boolean, Error[]>>;
  verifyAccount: (token: string) => Promise<Result<boolean, Error[]>>;
}

export class AuthService implements IAuthService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly client: IClient;
  private readonly endpoints = {
    register: `${this.baseURL}/auth/register`,
    login: `${this.baseURL}/auth/login`,
    refreshToken: `${this.baseURL}/auth/refresh-token`,
    logout: `${this.baseURL}/auth/logout`,
    resendVerificationEmail: `${this.baseURL}/auth/resend-verification-email`,
    verifyAccount: `${this.baseURL}/auth/verify-account`,
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async verifyAccount(token: string) {
    const request = new ClientRequestWithBody(this.endpoints.verifyAccount, undefined, { token });

    try {
      const res = await this.client.post(request);

      if (res.status === 400) {
        return Err([new Error('Token is not valid. It is either expired or revoked.')]);
      }

      if (res.status === 404) {
        return Err([new Error('Token or account related to token was not found.')]);
      }

      if (res.status === 409) {
        return Err([new Error('User is already verified.')]);
      }

      if (res.ok === false) {
        return Err([new Error('Verifying account failed')]);
      }

      return Ok(true);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Verifying account failed')]);
    }
  }

  async resendVerificationEmail(email: string) {
    const request = new ClientRequestWithBody(
      this.endpoints.resendVerificationEmail,
      undefined,
      new ResendVerificationEmailRequest(email)
    );

    try {
      const res = await this.client.post(request);

      if (res.status === 400) {
        const body = await res.json();
        const validationErrors = body.errors as Record<string, string[]>;
        const errors = Object.values(validationErrors)
          .flat()
          .map(e => new Error(e));

        return Err(errors);
      }

      if (res.status === 404) {
        return Err([new Error('User not found. You may need to register.')]);
      }

      if (res.status === 409) {
        return Err([new Error('User is already verified.')]);
      }

      if (res.ok === false) {
        return Err([new Error('Resending verification email failed')]);
      }

      return Ok(true);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Resending verification email failed')]);
    }
  }

  async logout() {
    const request = new ClientRequestWithBody(this.endpoints.logout, undefined, undefined);

    try {
      const res = await this.client.post(request);

      if (res.ok === false) {
        return Err([new Error('Logout failed')]);
      }

      return Ok(true);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Logout failed')]);
    }
  }

  async refreshToken() {
    const request = new ClientRequestWithBody(this.endpoints.refreshToken, undefined, undefined);

    try {
      const res = await this.client.post(request);

      if (res.status === 401) {
        return Err([new Error('Refresh and/or access token is not valid')]);
      }

      if (res.ok === false) {
        return Err([new Error('Refreshing token failed')]);
      }

      const body = await res.json();
      return Ok(body as LoginResponse);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Refreshing token failed.')]);
    }
  }

  async login(email: string, password: string) {
    const request = new ClientRequestWithBody(
      this.endpoints.login,
      undefined,
      new LoginRequest(email, password)
    );

    try {
      const res = await this.client.post(request);

      if (res.status === 400) {
        const body = await res.json();
        const validationErrors = body.errors as Record<string, string[]>;
        const errors = Object.values(validationErrors)
          .flat()
          .map(e => new Error(e));

        return Err(errors);
      }

      if (res.status === 401) {
        return Err([new Error('Email/Password combination is not valid')]);
      }

      if (res.status === 403) {
        return Err([new UserNotVerifiedError()]);
      }

      if (res.ok === false) {
        return Err([new Error('Login failed. Please try again.')]);
      }

      const body = await res.json();
      return Ok(body as LoginResponse);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Login failed. Please try again.')]);
    }
  }

  async register(email: string, password: string) {
    const request = new ClientRequestWithBody(
      this.endpoints.register,
      undefined,
      new RegisterRequest(email, password)
    );

    try {
      const res = await this.client.post(request);

      if (res.status === 400) {
        const body = await res.json();
        const validationErrors = body.errors as Record<string, string[]>;
        const errors = Object.values(validationErrors)
          .flat()
          .map(e => new Error(e));

        return Err(errors);
      }

      if (res.status === 409) {
        return Err([new Error('Registration failed. User already exists with this email.')]);
      }

      if (res.ok === false) {
        return Err([new Error('Registration failed. Please try again.')]);
      }

      const body = await res.json();
      return Ok(body as RegisterResponse);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Registration failed. Please try again.')]);
    }
  }
}

export class UserNotVerifiedError extends Error {
  constructor() {
    super('User is not verified. Please verify your account.');
  }
}

class BaseAuthRequest {
  readonly email: string;
  readonly password: string;

  constructor(email: string, password: string) {
    this.email = email;
    this.password = password;
  }
}

class LoginRequest extends BaseAuthRequest {
  constructor(email: string, password: string) {
    super(email, password);
  }
}

class RegisterRequest extends BaseAuthRequest {
  constructor(email: string, password: string) {
    super(email, password);
  }
}

class ResendVerificationEmailRequest {
  readonly email: string;

  constructor(email: string) {
    this.email = email;
  }
}

type LoginResponse = {
  accessToken: string;
};

type RegisterResponse = {
  id: string;
};

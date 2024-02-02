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
}

export class AuthService implements IAuthService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly endpoints = {
    register: `${this.baseURL}/auth/register`,
    login: `${this.baseURL}/auth/login`,
  };
  private readonly client: IClient;

  constructor(client: IClient) {
    this.client = client;
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

      if (res.ok === false) {
        return Err([new Error('Login failed. Please try again.')]);
      }

      const body = await res.json();
      return Ok(body as LoginResponse);
    } catch (e) {
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

      if (res.ok === false) {
        return Err([new Error('Registration failed. Please try again.')]);
      }

      const body = await res.json();
      return Ok(body as RegisterResponse);
    } catch (e) {
      console.error(e);
      return Err([new Error('Registration failed. Please try again.')]);
    }
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

type LoginResponse = {
  accessToken: string;
};

type RegisterResponse = {
  id: string;
};

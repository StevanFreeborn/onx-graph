import { type InjectionKey } from 'vue';
import { type IClient } from './client';

type AuthServiceFactoryKeyType = InjectionKey<IAuthServiceFactory>;

export const AuthServiceFactoryKey: AuthServiceFactoryKeyType =
  Symbol('AuthServiceFactory');

export interface IAuthServiceFactory {
  create: (client: IClient) => IAuthService;
}

export class AuthServiceFactory implements IAuthServiceFactory {
  create(client: IClient): IAuthService {
    return new AuthService(client);
  }
}

export interface IAuthService {}

export class AuthService implements IAuthService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly endpoints = {
    login: `${this.baseURL}/auth/login`,
  };
  private readonly client: IClient;

  constructor(client: IClient) {
    this.client = client;
  }
}

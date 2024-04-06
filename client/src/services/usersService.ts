import { Err, Ok, Result } from 'ts-results';
import type { InjectionKey } from 'vue';
import { ClientRequestWithBody, type IClient } from './client';

type UsersServiceFactoryKeyType = InjectionKey<IUsersServiceFactory>;

export const UsersServiceFactoryKey: UsersServiceFactoryKeyType = Symbol('UsersServiceFactory');

export interface IUsersServiceFactory {
  create: (client: IClient) => IUsersService;
}

export class UsersServiceFactory implements IUsersServiceFactory {
  create(client: IClient): IUsersService {
    return new UsersService(client);
  }
}

export interface IUsersService {
  getUser: (userId: string) => Promise<Result<GetUserResponse, Error[]>>;
}

export class UsersService implements IUsersService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly client: IClient;

  private createGetUserEndpoint(userId: string) {
    return `${this.baseURL}/users/${userId}`;
  }

  private readonly endpoints = {
    getUser: (userId: string) => this.createGetUserEndpoint(userId),
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async getUser(userId: string) {
    const request = new ClientRequestWithBody(this.endpoints.getUser(userId), undefined, undefined);

    try {
      const res = await this.client.get(request);

      if (res.ok === false) {
        return Err([new Error('Failed to get user.')]);
      }

      const body = await res.json();

      return Ok(body as GetUserResponse);
    } catch (e) {
      console.error(e);
      return Err([new Error('Failed to get user.')]);
    }
  }
}

type GetUserResponse = {
  id: string;
  username: string;
};

import type { Graph, PageWithData } from '@/types';
import { Err, Ok, Result } from 'ts-results';
import type { InjectionKey } from 'vue';
import { ClientRequest, ClientRequestWithBody, type IClient } from './client';

type GraphsServiceFactoryKeyType = InjectionKey<IGraphsServiceFactory>;

export const GraphsServiceFactoryKey: GraphsServiceFactoryKeyType = Symbol('GraphsServiceFactory');

export interface IGraphsServiceFactory {
  create: (client: IClient) => IGraphsService;
}

export class GraphsServiceFactory implements IGraphsServiceFactory {
  create(client: IClient): IGraphsService {
    return new GraphsService(client);
  }
}

export interface IGraphsService {
  addGraph: (name: string, apiKey: string) => Promise<Result<AddGraphResponse, Error[]>>;
  getGraphs: (
    pageNumber?: number,
    pageSize?: number
  ) => Promise<Result<PageWithData<Graph>, Error[]>>;
}

export class GraphsService implements IGraphsService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly client: IClient;

  private createGetGraphsEndpoint(pageNumber: number, pageSize: number) {
    return `${this.baseURL}/graphs?pageNumber=${pageNumber}&pageSize=${pageSize}`;
  }

  private readonly endpoints = {
    addGraph: `${this.baseURL}/graphs/add`,
    getGraphs: (pageNumber: number, pageSize: number) =>
      this.createGetGraphsEndpoint(pageNumber, pageSize),
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async getGraphs(pageNumber: number = 1, pageSize: number = 50) {
    const request = new ClientRequest(this.endpoints.getGraphs(pageNumber, pageSize));

    try {
      const res = await this.client.get(request);

      if (res.ok === false) {
        return Err([new Error('Failed to get graphs.')]);
      }

      const body = await res.json();
      return Ok(body as PageWithData<Graph>);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to get graphs.')]);
    }
  }

  async addGraph(name: string, apiKey: string) {
    const request = new ClientRequestWithBody(this.endpoints.addGraph, undefined, { name, apiKey });

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
        return Err([new Error('Failed to add graph. User does not exist.')]);
      }

      if (res.status === 409) {
        return Err([new Error('Failed to add graph. Graph with this name already exists.')]);
      }

      if (res.ok === false) {
        return Err([new Error('Failed to add graph.')]);
      }

      const body = await res.json();
      return Ok(body as AddGraphResponse);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to add graph.')]);
    }
  }
}

type AddGraphResponse = {
  id: string;
};

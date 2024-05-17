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
  getGraph: (id: string) => Promise<Result<Graph, Error[]>>;
  getGraphKey: (id: string) => Promise<Result<GetGraphKeyResponse, Error[]>>;
  deleteGraph: (id: string) => Promise<Result<boolean, Error[]>>;
  updateGraph: (graph: Graph) => Promise<Result<Graph, Error[]>>;
}

export class GraphsService implements IGraphsService {
  private readonly baseURL = import.meta.env.VITE_API_BASE_URL;
  private readonly client: IClient;

  private createGetGraphsEndpoint(pageNumber: number, pageSize: number) {
    return `${this.baseURL}/graphs?pageNumber=${pageNumber}&pageSize=${pageSize}`;
  }

  private createGetGraphEndpoint(id: string) {
    return `${this.baseURL}/graphs/${id}`;
  }

  private createGetGraphKeyEndpoint(id: string) {
    return `${this.baseURL}/graphs/${id}/key`;
  }

  private createUpdateGraphEndpoint(id: string) {
    return `${this.baseURL}/graphs/${id}`;
  }

  private readonly endpoints = {
    addGraph: `${this.baseURL}/graphs/add`,
    getGraphs: (pageNumber: number, pageSize: number) =>
      this.createGetGraphsEndpoint(pageNumber, pageSize),
    getGraph: (id: string) => this.createGetGraphEndpoint(id),
    getGraphKey: (id: string) => this.createGetGraphKeyEndpoint(id),
    updateGraph: (id: string) => this.createUpdateGraphEndpoint(id),
  };

  constructor(client: IClient) {
    this.client = client;
  }

  async updateGraph(graph: Graph) {
    const request = new ClientRequestWithBody(
      this.endpoints.updateGraph(graph.id),
      undefined,
      graph
    );

    try {
      const res = await this.client.put(request);

      if (res.ok === false) {
        return Err([new Error('Failed to update graph.')]);
      }

      const body = await res.json();
      return Ok(body as Graph);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to update graph.')]);
    }
  }

  async deleteGraph(id: string) {
    const request = new ClientRequest(this.endpoints.getGraph(id));

    try {
      const res = await this.client.delete(request);

      if (res.ok === false) {
        return Err([new Error('Failed to delete graph.')]);
      }

      return Ok(true);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to delete graph.')]);
    }
  }

  async getGraphKey(id: string) {
    const request = new ClientRequest(this.endpoints.getGraphKey(id));

    try {
      const res = await this.client.get(request);

      if (res.ok === false) {
        return Err([new Error('Failed to get graph key.')]);
      }

      const body = await res.json();
      return Ok(body as GetGraphKeyResponse);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to get graph key.')]);
    }
  }

  async getGraph(id: string) {
    const request = new ClientRequest(this.endpoints.getGraph(id));

    try {
      const res = await this.client.get(request);

      if (res.status === 404) {
        return Err([new GraphNotFoundError()]);
      }

      if (res.ok === false) {
        return Err([new Error('Failed to get graph.')]);
      }

      const body = await res.json();
      return Ok(body as Graph);
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error(e);
      return Err([new Error('Failed to get graph.')]);
    }
  }

  async getGraphs(pageNumber: number = 1, pageSize: number = 30) {
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

type GetGraphKeyResponse = {
  key: string;
};

export class GraphNotFoundError extends Error {
  constructor() {
    super('Graph not found.');
  }
}

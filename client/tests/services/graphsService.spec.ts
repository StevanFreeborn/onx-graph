import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { GraphNotFoundError, GraphsService } from '../../src/services/graphsService';

describe('GraphsService', () => {
  let graphsService: GraphsService;

  const mockClient = {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    patch: vi.fn(),
  };

  beforeEach(() => {
    graphsService = new GraphsService(mockClient);
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should have a constructor that returns a new instance', () => {
    expect(graphsService).toBeInstanceOf(GraphsService);
  });

  it('should have an addGraph method', () => {
    expect(graphsService.addGraph).toBeInstanceOf(Function);
  });

  it('should have a getGraphs method', () => {
    expect(graphsService.getGraphs).toBeInstanceOf(Function);
  });

  it('should have a getGraph method', () => {
    expect(graphsService.getGraph).toBeInstanceOf(Function);
  });

  it('should have a getGraphKey method', () => {
    expect(graphsService.getGraphKey).toBeInstanceOf(Function);
  });

  it('should have a deleteGraph method', () => {
    expect(graphsService.deleteGraph).toBeInstanceOf(Function);
  });

  describe('addGraph', () => {
    it('should return an error if the response status is 400', async () => {
      mockClient.post.mockReturnValueOnce({
        status: 400,
        json: async () => ({ errors: { Name: ['Name is required'] } }),
      });

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Name is required')]);
    });

    it('should return an error if the response status is 404', async () => {
      mockClient.post.mockReturnValueOnce({ status: 404 });

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to add graph. User does not exist.')]);
    });

    it('should return an error if the response status is 409', async () => {
      mockClient.post.mockReturnValueOnce({ status: 409 });

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([
        new Error('Failed to add graph. Graph with this name already exists.'),
      ]);
    });

    it('should return an error if the response status is not ok', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.post.mockReturnValueOnce({ ok: false });

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to add graph.')]);
    });

    it('should return an error if the request fails', async () => {
      mockClient.post.mockRejectedValueOnce(new Error('Failed to add graph.'));

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to add graph.')]);
    });

    it('should return the response body if the request is successful', async () => {
      mockClient.post.mockReturnValueOnce({
        ok: true,
        status: 201,
        json: async () => ({ id: '123' }),
      });

      const result = await graphsService.addGraph('graph', 'key');

      expect(result.err).toBe(false);
      expect(result.val).toEqual({ id: '123' });
    });
  });

  describe('getGraphs', () => {
    it('should return an error if the response status is not ok', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.get.mockReturnValueOnce({ ok: false });

      const result = await graphsService.getGraphs(1, 10);

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graphs.')]);
    });

    it('should return an error if the request fails', async () => {
      mockClient.get.mockRejectedValueOnce(new Error('Failed to get graphs.'));

      const result = await graphsService.getGraphs(1, 10);

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graphs.')]);
    });

    it('should return the response body if the request is successful', async () => {
      const page = { pageCount: 0, pageNumber: 1, totalCount: 0, totalPages: 0, data: [] };

      mockClient.get.mockReturnValueOnce({
        ok: true,
        json: async () => page,
      });

      const result = await graphsService.getGraphs(1, 10);

      expect(result.err).toBe(false);
      expect(result.val).toEqual(page);
    });
  });

  describe('getGraph', () => {
    it('should return a graph not found error if the response status is 404', async () => {
      mockClient.get.mockReturnValueOnce({ status: 404 });

      const result = await graphsService.getGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new GraphNotFoundError()]);
    });

    it('should return an error if the response status is not ok', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.get.mockReturnValueOnce({ ok: false });

      const result = await graphsService.getGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graph.')]);
    });

    it('should return an error if the request fails', async () => {
      mockClient.get.mockRejectedValueOnce(new Error('Failed to get graph.'));

      const result = await graphsService.getGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graph.')]);
    });

    it('should return the response body if the request is successful', async () => {
      const graph = { id: '123', name: 'graph', apiKey: 'key' };

      mockClient.get.mockReturnValueOnce({
        ok: true,
        json: async () => graph,
      });

      const result = await graphsService.getGraph('123');

      expect(result.err).toBe(false);
      expect(result.val).toEqual(graph);
    });
  });

  describe('getGraphKey', () => {
    it('should return an error if the response status is not ok', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.get.mockReturnValueOnce({ ok: false });

      const result = await graphsService.getGraphKey('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graph key.')]);
    });

    it('should return an error if the request fails', async () => {
      mockClient.get.mockRejectedValueOnce(new Error('Failed to get graph key.'));

      const result = await graphsService.getGraphKey('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get graph key.')]);
    });

    it('should return the response body if the request is successful', async () => {
      mockClient.get.mockReturnValueOnce({
        ok: true,
        json: async () => ({ key: 'key' }),
      });

      const result = await graphsService.getGraphKey('123');

      expect(result.err).toBe(false);
      expect(result.unwrap().key).toEqual('key');
    });
  });

  describe('deleteGraph', () => {
    it('should return an error if the response status is not ok', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.delete.mockReturnValueOnce({ ok: false });

      const result = await graphsService.deleteGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to delete graph.')]);
    });

    it('should return an error if the request fails', async () => {
      mockClient.delete.mockRejectedValueOnce(new Error('Failed to delete graph.'));

      const result = await graphsService.deleteGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to delete graph.')]);
    });

    it('should return true if the request is successful', async () => {
      mockClient.delete.mockReturnValueOnce({ ok: true });

      const result = await graphsService.deleteGraph('123');

      expect(result.err).toBe(false);
      expect(result.val).toBe(true);
    });
  });

  describe('updateGraph', () => {
    it('should return an error if the response is unsuccessful', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.put.mockReturnValueOnce({ ok: false });

      const result = await graphsService.updateGraph({});

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to update graph.')]);
    });

    it('should return an error if the request fails', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.put.mockRejectedValueOnce(new Error('Failed to update graph.'));

      const result = await graphsService.updateGraph({});

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to update graph.')]);
    });

    it('should return the response body if the request is successful', async () => {
      mockClient.put.mockReturnValueOnce({
        ok: true,
        json: async () => ({ id: '123' }),
      });

      const result = await graphsService.updateGraph({ id: '123' });

      expect(result.err).toBe(false);
      expect(result.val).toEqual({ id: '123' });
    });
  });

  describe('updateGraphKey', () => {
    it('should return an error if the response is unsuccessful', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.patch.mockReturnValueOnce({ ok: false });

      const result = await graphsService.updateGraphKey('123', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to update graph key.')]);
    });

    it('should return an error if the request fails', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.patch.mockRejectedValueOnce(new Error('Failed to update graph key.'));

      const result = await graphsService.updateGraphKey('123', 'key');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to update graph key.')]);
    });

    it('should return true if the request is successful', async () => {
      mockClient.patch.mockReturnValueOnce({ ok: true });

      const result = await graphsService.updateGraphKey('123', 'key');

      expect(result.err).toBe(false);
      expect(result.val).toBe(true);
    });
  });

  describe('refreshGraph', () => {
    it('should return an error if the response is unsuccessful', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.patch.mockReturnValueOnce({ ok: false });

      const result = await graphsService.refreshGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to refresh graph.')]);
    });

    it('should return an error if the request fails', async () => {
      vi.spyOn(console, 'error').mockImplementationOnce(() => {});

      mockClient.patch.mockRejectedValueOnce(new Error('Failed to refresh graph.'));

      const result = await graphsService.refreshGraph('123');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to refresh graph.')]);
    });

    it('should return true if the request is successful', async () => {
      mockClient.patch.mockReturnValueOnce({ ok: true });

      const result = await graphsService.refreshGraph('123');

      expect(result.err).toBe(false);
      expect(result.val).toBe(true);
    });
  });
});

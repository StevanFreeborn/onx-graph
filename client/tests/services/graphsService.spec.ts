import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { GraphsService } from '../../src/services/graphsService';

describe('GraphsService', () => {
  let graphsService: GraphsService;

  const mockClient = {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
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
});

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

      mockClient.post.mockReturnValueOnce({ status: 500 });

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
});

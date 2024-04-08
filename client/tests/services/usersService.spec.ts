import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { mockClient } from '../testUtils';
import { UsersService } from './../../src/services/usersService';

describe('UsersService', () => {
  let usersService: UsersService;

  beforeEach(() => {
    usersService = new UsersService(mockClient);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should have a constructor that returns a new instance', () => {
    expect(usersService).toBeInstanceOf(UsersService);
  });

  it('should have a getUser method', () => {
    expect(usersService.getUser).toBeInstanceOf(Function);
  });

  describe('getUser', () => {
    it('should return an error if the request fails', async () => {
      mockClient.get.mockResolvedValue({ ok: false });

      const result = await usersService.getUser('1');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get user.')]);
    });

    it('should return the user if the request is successful', async () => {
      mockClient.get.mockResolvedValue({
        ok: true,
        json: vi.fn().mockResolvedValue({ id: '1', username: 'test' }),
      });

      const result = await usersService.getUser('1');

      expect(result.ok).toBe(true);
      expect(result.unwrap()).toEqual({ id: '1', username: 'test' });
    });

    it('should return an error if the request throws an error', async () => {
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      mockClient.get.mockRejectedValue(new Error('Failed to get user.'));

      const result = await usersService.getUser('1');

      expect(result.err).toBe(true);
      expect(result.val).toEqual([new Error('Failed to get user.')]);
      expect(consoleSpy).toHaveBeenCalledWith(new Error('Failed to get user.'));
    });
  });
});

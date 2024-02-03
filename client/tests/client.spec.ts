import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Client, ClientConfig, ClientRequestWithBody } from './../src/services/client';

describe('Client', () => {
  let client: Client;
  let clientConfig: ClientConfig;
  const fetchMock = vi.fn();

  beforeEach(() => {
    clientConfig = new ClientConfig(undefined, true, undefined);
    client = new Client(clientConfig);
    global.fetch = fetchMock;
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  it('should have a constructor that returns a new instance', () => {
    expect(client).toBeInstanceOf(Client);
  });

  it('should have a get method', () => {
    expect(client.get).toBeInstanceOf(Function);
  });

  it('should have a post method', () => {
    expect(client.post).toBeInstanceOf(Function);
  });

  it('should have a put method', () => {
    expect(client.put).toBeInstanceOf(Function);
  });

  it('should have a delete method', () => {
    expect(client.delete).toBeInstanceOf(Function);
  });

  describe('get', () => {
    it('should make a get request using fetch using given url and config', async () => {
      const url = 'http://example.com';
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.get({ url });

      expect(fetchMock).toHaveBeenCalledWith(
        new Request(url, { method: 'GET', credentials: 'include' })
      );
      expect(result).toBe(response);
    });
  });

  describe('post', () => {
    it('should make a post request using fetch using given url and config', async () => {
      const url = 'http://example.com/';
      const body = { data: 'test' };
      const request = new ClientRequestWithBody(url, undefined, body);
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.post(request);

      expect(fetchMock).toHaveBeenCalledWith(
        expect.objectContaining({
          method: 'POST',
          url: url,
          body: expect.any(Object),
        })
      );
      expect(result).toBe(response);
    });
  });

  describe('put', () => {
    it('should make a put request using fetch using given url and config', async () => {
      const url = 'http://example.com/';
      const body = { data: 'test' };
      const request = new ClientRequestWithBody(url, undefined, body);
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.put(request);

      expect(fetchMock).toHaveBeenCalledWith(
        expect.objectContaining({
          method: 'PUT',
          url: url,
          body: expect.any(Object),
        })
      );
      expect(result).toBe(response);
    });
  });

  describe('delete', () => {
    it('should make a delete request using fetch using given url and config', async () => {
      const url = 'http://example.com';
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.delete({ url });

      expect(fetchMock).toHaveBeenCalledWith(
        new Request(url, { method: 'DELETE', credentials: 'include' })
      );
      expect(result).toBe(response);
    });
  });
});

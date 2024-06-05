import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { Client, ClientConfig, ClientRequestWithBody } from '../../src/services/client';

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

  it('should retry requests if given an unauthorized response handler when a 401 status is returned', async () => {
    const accessToken = 'token';
    const retryResponse = new Response();
    const unauthorizedResponseHandler = vi
      .fn()
      .mockReturnValueOnce({ response: retryResponse, accessToken });
    const clientConfig = new ClientConfig(undefined, true, unauthorizedResponseHandler);
    const client = new Client(clientConfig);

    fetchMock.mockReturnValueOnce({ status: 401 });

    await client.get({ url: 'http://example.com' });

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(unauthorizedResponseHandler).toHaveBeenCalledWith(expect.any(Request));
  });

  it('should not include credentials in the request if includeCredentials is false', async () => {
    const clientConfig = new ClientConfig(undefined, false, undefined);
    const client = new Client(clientConfig);

    fetchMock.mockReturnValueOnce(new Response());

    await client.get({ url: 'http://example.com' });

    expect(fetchMock).toHaveBeenCalledWith(expect.objectContaining({ credentials: 'omit' }));
  });

  it('should display alert if request encounters a 429 status', async () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});

    const client = new Client(new ClientConfig());

    fetchMock.mockReturnValueOnce(
      new Response(undefined, {
        status: 429,
      })
    );

    await client.get({ url: 'http://example.com' });

    expect(alert).toHaveBeenCalledWith(expect.any(String));
  });

  it('should display alert that includes retry time if request encounters a 429 status and X-Retry-After is present', async () => {
    vi.spyOn(window, 'alert').mockImplementation(() => {});

    const client = new Client(new ClientConfig());

    const retryAfterDateString = '2022-01-01T00:00:00Z';
    const expectedDateString = new Date(retryAfterDateString).toLocaleTimeString();

    fetchMock.mockReturnValueOnce(
      new Response(undefined, {
        headers: { 'X-Retry-After': retryAfterDateString },
        status: 429,
      })
    );

    await client.get({ url: 'http://example.com' });

    expect(alert).toHaveBeenCalledWith(expect.stringContaining(expectedDateString));
  });

  describe('get', () => {
    it('should make a get request using fetch using given url and config', async () => {
      const url = 'http://example.com';
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.get({ url });

      expect(fetchMock).toHaveBeenCalledWith(
        expect.objectContaining({ method: 'GET', credentials: 'include' })
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
        expect.objectContaining({ method: 'DELETE', credentials: 'include' })
      );
      expect(result).toBe(response);
    });
  });

  describe('patch', () => {
    it('should make a patch request using fetch using given url and config', async () => {
      const url = 'http://example.com/';
      const body = { data: 'test' };
      const request = new ClientRequestWithBody(url, undefined, body);
      const response = new Response();
      fetchMock.mockReturnValue(response);

      const result = await client.patch(request);

      expect(fetchMock).toHaveBeenCalledWith(
        expect.objectContaining({
          method: 'PATCH',
          url: url,
          body: expect.any(Object),
        })
      );

      expect(result).toBe(response);
    });
  });
});

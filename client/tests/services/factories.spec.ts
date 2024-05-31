import { describe, expect, it, vi } from 'vitest';
import { AuthService, AuthServiceFactory } from '../../src/services/authService';
import { Client, ClientConfig, ClientFactory } from '../../src/services/client';
import { GraphsService, GraphsServiceFactory } from '../../src/services/graphsService';
import { UsersService, UsersServiceFactory } from '../../src/services/usersService';

const mockClient = {
  post: vi.fn(),
  get: vi.fn(),
  put: vi.fn(),
  delete: vi.fn(),
  patch: vi.fn(),
};

describe('ClientFactory', () => {
  it('should have a create method', () => {
    const factory = new ClientFactory();
    expect(factory.create).toBeInstanceOf(Function);
  });

  describe('create', () => {
    it('should return a Client instance without a config', () => {
      const factory = new ClientFactory();
      const client = factory.create();
      expect(client).toBeInstanceOf(Client);
    });

    it('should return a Client instance with a config', () => {
      const factory = new ClientFactory();
      const client = factory.create(new ClientConfig());
      expect(client).toBeInstanceOf(Client);
    });
  });
});

describe('GraphsServiceFactory', () => {
  it('should have a create method', () => {
    const factory = new GraphsServiceFactory();
    expect(factory.create).toBeInstanceOf(Function);
  });

  describe('create', () => {
    it('should return a GraphsService instance', () => {
      const factory = new GraphsServiceFactory();
      const service = factory.create(mockClient);
      expect(service).toBeInstanceOf(GraphsService);
    });
  });
});

describe('AuthServiceFactory', () => {
  it('should have a create method', () => {
    const factory = new AuthServiceFactory();
    expect(factory.create).toBeInstanceOf(Function);
  });

  describe('create', () => {
    it('should return a AuthService instance', () => {
      const factory = new AuthServiceFactory();
      const service = factory.create(mockClient);
      expect(service).toBeInstanceOf(AuthService);
    });
  });
});

describe('UsersServiceFactory', () => {
  it('should have a create method', () => {
    const factory = new UsersServiceFactory();
    expect(factory.create).toBeInstanceOf(Function);
  });

  describe('create', () => {
    it('should return a UsersService instance', () => {
      const factory = new UsersServiceFactory();
      const service = factory.create(mockClient);
      expect(service).toBeInstanceOf(UsersService);
    });
  });
});

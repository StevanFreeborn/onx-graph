import { ClientConfig, ClientFactoryKey } from '@/services/client';
import { UsersServiceFactoryKey } from '@/services/usersService';
import type { UserStore } from '@/stores/userStore';
import { inject } from 'vue';

export function useUsersService(store?: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const usersServiceFactory = inject(UsersServiceFactoryKey);

  if (!clientFactory || !usersServiceFactory) {
    throw new Error('Failed to inject dependencies');
  }

  const client = store
    ? clientFactory.create(
        new ClientConfig(
          { Authorization: `Bearer ${store.user?.token}` },
          true,
          store.refreshAccessToken
        )
      )
    : clientFactory.create();

  const usersService = usersServiceFactory.create(client);

  return usersService;
}

import { ClientConfig, ClientFactoryKey } from '@/services/client';
import type { UserStore } from '@/stores/userStore';
import { inject } from 'vue';
import { AuthServiceFactoryKey } from './../services/authService';

export function useAuthService(store?: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const authServiceFactory = inject(AuthServiceFactoryKey);

  if (!clientFactory || !authServiceFactory) {
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

  const authService = authServiceFactory.create(client);

  return authService;
}

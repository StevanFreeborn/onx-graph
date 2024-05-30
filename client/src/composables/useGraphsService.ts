import { ClientConfig, ClientFactoryKey } from '@/services/client';
import { GraphsServiceFactoryKey } from '@/services/graphsService';
import type { UserStore } from '@/stores/userStore';
import { inject } from 'vue';

export function useGraphsService(store?: UserStore) {
  const clientFactory = inject(ClientFactoryKey);
  const graphsServiceFactory = inject(GraphsServiceFactoryKey);

  if (!clientFactory || !graphsServiceFactory) {
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

  const graphsService = graphsServiceFactory.create(client);

  return graphsService;
}

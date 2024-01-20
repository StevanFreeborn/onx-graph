import { ClientFactoryKey } from '@/services/client';
import { inject } from 'vue';
import { AuthServiceFactoryKey } from './../services/authService';

export function useAuthService() {
  const clientFactory = inject(ClientFactoryKey);
  const authServiceFactory = inject(AuthServiceFactoryKey);

  if (!clientFactory || !authServiceFactory) {
    throw new Error('Failed to inject dependencies');
  }

  const client = clientFactory.create();
  const authService = authServiceFactory.create(client);

  return authService;
}

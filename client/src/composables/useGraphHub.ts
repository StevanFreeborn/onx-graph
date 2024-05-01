import { useUserStore } from '@/stores/userStore';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthService } from './useAuthService';

export function useGraphHub() {
  const userStore = useUserStore();
  const authService = useAuthService(userStore);
  const baseUrl = import.meta.env.VITE_API_BASE_URL;

  const connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/graphs/hub`, {
      accessTokenFactory: async () => {
        const token = userStore.user?.token ?? '';
        const expiresAt = userStore.user?.expiresAtInSeconds;
        const expiresAtInMs = expiresAt ? expiresAt * 1000 : 0;
        const now = new Date().getTime();

        if (expiresAtInMs > now) {
          return token;
        }

        const refreshResult = await authService.refreshToken();

        if (refreshResult.err) {
          for (const error of refreshResult.val) {
            // eslint-disable-next-line no-console
            console.error(error);
          }

          return token;
        }

        userStore.logUserIn(refreshResult.val.accessToken);
        return refreshResult.val.accessToken;
      },
    })
    .withAutomaticReconnect()
    .build();

  return connection;
}

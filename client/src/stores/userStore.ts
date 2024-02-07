import { AuthServiceFactoryKey } from '@/services/authService';
import { ClientConfig, ClientFactoryKey } from '@/services/client';
import { jwtDecode } from 'jwt-decode';
import { defineStore } from 'pinia';
import { inject, ref } from 'vue';

export const USER_KEY = 'onxGraphAuth';

export type User = {
  id: string;
  expiresAtInSeconds: number;
  token: string;
};

type JwtTokenPayload = {
  sub: string;
  exp: number;
};

function getUserFromLocalStorage(): User | null {
  const user = localStorage.getItem(USER_KEY);
  return user === null ? null : JSON.parse(user);
}

export type UserStore = ReturnType<typeof useUserStore>;

export const useUserStore = defineStore('userStore', () => {
  const user = ref<User | null>(getUserFromLocalStorage());
  const clientFactory = inject(ClientFactoryKey);
  const authServiceFactory = inject(AuthServiceFactoryKey);

  function logUserIn(jwtToken: string) {
    const { sub, exp } = jwtDecode<JwtTokenPayload>(jwtToken);
    const loggedInUser: User = {
      id: sub,
      expiresAtInSeconds: exp,
      token: jwtToken,
    };
    localStorage.setItem(USER_KEY, JSON.stringify(loggedInUser));
    user.value = loggedInUser;
  }

  function logUserOut() {
    localStorage.removeItem(USER_KEY);
    user.value = null;
    console.log(user.value);
  }

  async function refreshAccessToken(originalRequest: Request) {
    const unauthorizedResponse = new Response(null, { status: 401 });

    if (user.value === null || !clientFactory || !authServiceFactory) {
      logUserOut();
      return unauthorizedResponse;
    }

    const client = clientFactory.create(
      new ClientConfig({ Authorization: `Bearer ${user.value.token}` }, true)
    );

    const authService = authServiceFactory.create(client);

    const refreshResult = await authService.refreshToken();

    if (refreshResult.err) {
      logUserOut();
      return unauthorizedResponse;
    }

    logUserIn(refreshResult.val.accessToken);

    originalRequest.headers.set('Authorization', `Bearer ${refreshResult.val.accessToken}`);

    return await fetch(originalRequest);
  }

  return {
    user: user,
    logUserIn,
    logUserOut,
    refreshAccessToken,
  };
});

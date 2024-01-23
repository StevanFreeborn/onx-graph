import { jwtDecode } from 'jwt-decode';
import { defineStore } from 'pinia';
import { readonly, ref } from 'vue';
import { useRouter } from 'vue-router';

const USER_KEY = 'onxGraphAuth';

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

export const useUserStore = defineStore('userStore', () => {
  const user = ref<User | null>(getUserFromLocalStorage());
  const router = useRouter();

  function logUserIn(jwtToken: string) {
    const { sub, exp } = jwtDecode<JwtTokenPayload>(jwtToken);
    const loggedInUser: User = {
      id: sub,
      expiresAtInSeconds: exp,
      token: jwtToken,
    };
    localStorage.setItem(USER_KEY, JSON.stringify(loggedInUser));
    user.value = loggedInUser;
    router.push('/');
  }

  return {
    user: readonly(user),
    logUserIn,
  };
});

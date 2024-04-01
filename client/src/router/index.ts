import { AuthService } from '@/services/authService';
import { Client, ClientConfig } from '@/services/client';
import { useUserStore } from '@/stores/userStore';
import { createRouter, createWebHistory } from 'vue-router';

export const routes = [
  {
    path: '/masses',
    name: 'masses',
    redirect: { name: 'home' },
    component: () => import('../components/PublicLayout.vue'),
    beforeEnter: () => {
      const { user } = useUserStore();

      return user ? { name: 'graphs' } : true;
    },
    children: [
      {
        path: 'home',
        name: 'home',
        component: () => import('../views/HomeView.vue'),
      },
      {
        path: 'login',
        name: 'login',
        component: () => import('../views/LoginView.vue'),
      },
      {
        path: 'unverified',
        name: 'unverified',
        component: () => import('../views/UnverifiedView.vue'),
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('../views/RegisterView.vue'),
      },
      {
        path: 'register-confirmation',
        name: 'register-confirmation',
        component: () => import('../views/RegisterConfirmationView.vue'),
      },
      {
        path: 'about',
        name: 'about',
        component: () => import('../views/AboutView.vue'),
      },
      {
        path: 'verify-account',
        name: 'verify-account',
        component: () => import('../views/VerifyAccountView.vue'),
      },
      {
        path: '/:pathMatch(.*)*',
        // name: 'not-found',
        component: () => import('../views/NotFoundView.vue'),
      },
    ],
  },
  {
    path: '/',
    name: 'root',
    redirect: { name: 'graphs' },
    beforeEnter: async () => {
      const userStore = useUserStore();

      if (userStore.user === null) {
        return { name: 'home' };
      }

      const isExpired = userStore.user.expiresAtInSeconds < Date.now() / 1000;

      if (isExpired) {
        // can't use useAuthService here because
        // it uses inject and inject can only be
        // used in the setup function of a component
        const clientConfig = new ClientConfig(
          { Authorization: `Bearer ${userStore.user.token}` },
          true
        );
        const client = new Client(clientConfig);
        const authService = new AuthService(client);
        const refreshResult = await authService.refreshToken();

        if (refreshResult.err) {
          // not posting to server to log out
          // because at this point both the
          // refresh token and the access token
          // are not valid
          console.error(refreshResult.err);
          userStore.logUserOut();
          return { name: 'login' };
        }

        userStore.logUserIn(refreshResult.val.accessToken);
      }

      return true;
    },
    children: [
      {
        path: 'graphs',
        name: 'graphs',
        component: () => import('../views/GraphsView.vue'),
      },
      {
        path: '/:pathMatch(.*)*',
        name: 'not-found',
        component: () => import('../views/NotFoundView.vue'),
      },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routes,
});

// TODO: Add actual error handling for router
router.onError(error => {
  console.error(error);
});

export default router;

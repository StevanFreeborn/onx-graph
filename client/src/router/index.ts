import { useUserStore } from '@/stores/userStore';
import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
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
          path: 'register',
          name: 'register',
          component: () => import('../views/RegisterView.vue'),
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/AboutView.vue'),
        },
        {
          path: '/:pathMatch(.*)*',
          name: 'not-found',
          component: () => import('../views/NotFoundView.vue'),
        },
      ],
    },
    {
      path: '/',
      name: 'root',
      redirect: { name: 'graphs' },
      beforeEnter: () => {
        const { user } = useUserStore();

        if (user === null) {
          return { name: 'home' };
        }

        const isExpired = user.expiresAtInSeconds < Date.now() / 1000;

        if (isExpired) {
          // TODO: Make request to server to attempt to refresh token
          // If successful, update user store and continue
          // if not, post to server to log user out and then
          // log user out in the client and redirect to login
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
  ],
});

export default router;

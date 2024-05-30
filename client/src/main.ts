import 'v-network-graph/lib/style.css';
import './assets/main.css';

import { createPinia } from 'pinia';
import VNetworkGraph from 'v-network-graph';
import { createApp } from 'vue';

import App from './App.vue';
import router from './router';

import { AuthServiceFactory, AuthServiceFactoryKey } from './services/authService';
import { ClientFactory, ClientFactoryKey } from './services/client';
import { GraphsServiceFactory, GraphsServiceFactoryKey } from './services/graphsService';
import { UsersServiceFactory, UsersServiceFactoryKey } from './services/usersService';

const app = createApp(App);

app.provide(ClientFactoryKey, new ClientFactory());
app.provide(AuthServiceFactoryKey, new AuthServiceFactory());
app.provide(UsersServiceFactoryKey, new UsersServiceFactory());
app.provide(GraphsServiceFactoryKey, new GraphsServiceFactory());

app.use(createPinia());

app.use(router);

app.use(VNetworkGraph);

// TODO: Add actual global error handling for the app
app.config.errorHandler = (err, vm, info) => {
  // eslint-disable-next-line no-console
  console.error(err, vm, info);
};

app.mount('#app');

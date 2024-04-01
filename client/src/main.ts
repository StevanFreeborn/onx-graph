import 'v-network-graph/lib/style.css';
import './assets/main.css';

import { createPinia } from 'pinia';
import VNetworkGraph from 'v-network-graph';
import { createApp } from 'vue';

import App from './App.vue';
import router from './router';

import { AuthServiceFactory, AuthServiceFactoryKey } from './services/authService';
import { ClientFactory, ClientFactoryKey } from './services/client';

const app = createApp(App);

app.provide(ClientFactoryKey, new ClientFactory());
app.provide(AuthServiceFactoryKey, new AuthServiceFactory());

app.use(createPinia());

app.use(router);

app.use(VNetworkGraph);

// TODO: Add actual global error handling for the app
app.config.errorHandler = (err, vm, info) => {
  console.error(err, vm, info);
};

app.mount('#app');

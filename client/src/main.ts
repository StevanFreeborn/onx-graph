import 'v-network-graph/lib/style.css';
import './assets/main.css';

import { createPinia } from 'pinia';
import VNetworkGraph from 'v-network-graph';
import { createApp } from 'vue';

import App from './App.vue';
import router from './router';

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(VNetworkGraph);

app.mount('#app');

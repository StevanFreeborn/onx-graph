import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';
import fs from 'fs';
import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    port: 3001,
    watch: {
      usePolling: true,
      interval: 1000,
    },
    https: {
      cert: fs.readFileSync('./.certs/cert.pem'),
      key: fs.readFileSync('./.certs/key.pem'),
    },
  },
  plugins: [vue(), vueJsx()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
});

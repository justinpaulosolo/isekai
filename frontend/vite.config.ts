import tailwindcss from '@tailwindcss/vite';
import { tanstackRouter } from '@tanstack/router-plugin/vite';
import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';
const target: string | undefined = process.env.SERVER_HTTP ?? process.env.SERVER_HTTPS;
// https://vite.dev/config/
export default defineConfig({
  plugins: [
    tanstackRouter({
      routesDirectory: './src/routes',
      generatedRouteTree: './src/routeTree.gen.ts',
    }),
    react(),
    tailwindcss(),
  ],
  server: {
    port: parseInt(process.env.PORT ?? '5173'), // keep in sync with AppHost + Google console
    strictPort: true,
    proxy: {
      '/api': { target, secure: false, xfwd: true }, // no changeOrigin
      '/signin-google': { target, secure: false, xfwd: true },
    },
  },
});

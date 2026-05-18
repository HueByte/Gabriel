import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 6080,
    proxy: {
      // Forward /api to the .NET backend during dev so the browser sees a same-origin call.
      '/api': 'http://localhost:6040',
    },
  },
});

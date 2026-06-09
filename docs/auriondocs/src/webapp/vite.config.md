# vite.config

> **File:** `src/webapp/vite.config.ts`  
> **Kind:** file

Exports the Vite configuration used by the webapp during development. It enables the React plugin (JSX transform and fast refresh), runs the dev server on port 6080, and forwards any requests whose path begins with /api to the backend at http://localhost:6040 so the browser observes same-origin calls during local development.

## Remarks
This file exists to encapsulate developer-facing behavior that differs from production: the dev-time proxy avoids CORS and lets frontend code call /api without switching base URLs while developing locally. The react plugin is included to provide the standard React handling (JSX transform, HMR/fast refresh) that Vite supplies for React projects.

## Example
```typescript
// Typical client-side call that benefits from the dev proxy
fetch('/api/todos')
  .then(r => r.json())
  .then(data => console.log(data));
```

// If your backend expects requests without the /api prefix, you can add a rewrite:
```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 6080,
    proxy: {
      '/api': {
        target: 'http://localhost:6040',
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
});
```

## Notes
- The proxy only applies to the Vite development server; production builds must be configured separately to point the client at the correct backend.
- By default the proxy preserves the /api prefix; add a rewrite if the backend expects routes without it.
- Changing this file requires restarting the Vite dev server for changes to take effect; also ensure ports 6080 (frontend) and 6040 (backend) are available to avoid conflicts.

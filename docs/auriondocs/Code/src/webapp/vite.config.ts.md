# vite.config

> **File:** `src/webapp/vite.config.ts`  
> **Kind:** file


Configures the frontend development environment for a React app built with Vite, enabling the React plugin, running the dev server on port 6080, and proxying API requests under /api to the local .NET backend to simplify development and avoid CORS issues.

## Remarks
This abstraction centralizes development-time behavior in a single place. By proxying /api to the backend, it allows frontend code to call /api endpoints as if they were same-origin, while production would typically talk to a real backend URL defined elsewhere. The config is intentionally scoped to development and should be complemented by environment-specific settings for builds.

## Example
```typescript
// Common variant: proxy API calls to a backend during dev
export default defineConfig({
  plugins: [react()],
  server: {
    port: 6080,
    proxy: {
      '/api': 'http://localhost:6040',
    },
  },
});
```

## Notes
- This proxy configuration only applies to the Vite development server; production builds will not proxy requests through this configuration.
- Ensure the backend URL and port are correct and accessible during development; adjust the proxy targets as needed and restart the dev server after changes.
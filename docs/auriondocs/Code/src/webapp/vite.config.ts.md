# vite.config

> **File:** `src/webapp/vite.config.ts`  
> **Kind:** file


Proposes a Vite configuration that wires React support and a development-time proxy. It exports a config with the React plugin enabled and a development server configured to listen on port 6080. All requests to /api are proxied to http://localhost:6040, so frontend code can call backend endpoints using a single origin during development without CORS headaches.

## Remarks
This abstraction centralizes the development-time wiring of the frontend and backend. By keeping the API path under /api, the frontend remains decoupled from the exact backend host while still preserving a realistic request path. The React plugin integration ensures fast refresh and seamless JSX/TSX handling during development.

## Example
```typescript
// Example: a typical frontend call to the backend API during development
fetch('/api/users')
  .then((r) => r.json())
  .then((data) => console.log(data))
  .catch((e) => console.error(e));
```

## Notes
- This proxy is active only in development; production builds should use the backend host directly or a separate reverse-proxy.
- If the backend port or path changes, update the proxy target accordingly.
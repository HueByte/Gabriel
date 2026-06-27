# vite.config

> **File:** `src/webapp/vite.config.ts`  
> **Kind:** file

Sets up the Vite development configuration for the web application: enables the React plugin and configures the dev server to run on port 6080 with a proxy that forwards requests under /api to the local .NET backend at http://localhost:6040. Use this file when running the Vite dev server locally so the browser sees API calls as same-origin and you get React fast-refresh and JSX handling from the plugin.

## Remarks
This configuration exists to simplify local development: the React plugin integrates JSX and Fast Refresh into Vite, while the dev-server proxy keeps browser requests for /api on the same origin and forwards them to the backend, avoiding CORS during development. The file exports a typed configuration via defineConfig so IDEs and type-checking get better diagnostics.

## Notes
- The proxy mapping preserves the path by default: a request to /api/users will be forwarded to http://localhost:6040/api/users. If the backend does not expect the /api prefix, add a rewrite rule to remove it.
- This only affects the development server. Production deployments must handle API routing or CORS on the real server (Vite build output does not include this proxy behavior).
- Ensure port 6080 (dev server) and 6040 (backend) are free or update the ports to avoid conflicts.
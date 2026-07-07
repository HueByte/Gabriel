# vite.config

> **File:** `src/webapp/vite.config.ts`  
> **Kind:** file


This file contains the Vite configuration for the web frontend. It enables the React plugin and exports a configuration object that runs the development server on port 6080 and proxies API calls under /api to the .NET backend at http://localhost:6040. This setup lets the frontend call /api endpoints during development as if they were same-origin, avoiding CORS issues while keeping backend and frontend decoupled.

## Remarks
Centralizing this dev-time configuration makes it easy to switch environments without touching application code. The proxy keeps the frontend code agnostic of the backend location; by adjusting the target in this file, you can redirect API requests to a different backend or host without changing call sites.

## Notes
- The proxy depends on the backend being reachable at the target address during development; if the backend isn't running, API requests will fail.
- The proxy is only active in Vite's dev server; production builds won't proxy requests. For production, configure the real API base URL or a different proxy strategy.
- If you modify the port or API path, update this file accordingly to preserve the same development ergonomics.
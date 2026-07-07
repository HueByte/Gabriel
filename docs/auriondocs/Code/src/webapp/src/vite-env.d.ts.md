# vite-env.d

> **File:** `src/webapp/src/vite-env.d.ts`  
> **Kind:** file


This file pulls in the Vite client type definitions into the global TypeScript scope, allowing the compiler to understand Vite-specific globals without importing runtime code. It’s a standard part of Vite projects to provide typed access to environment variables and HMR-related APIs via import.meta and related Vite runtime features across all modules.

## Remarks
Vite-env.d.ts serves as a central source of ambient typings for the client environment. By referencing vite/client, it ensures consistent IntelliSense for import.meta.env and Vite's runtime shims across the codebase. This decouples type information from implementation details, so modules can read environment values and HMR signals without importing Vite-specific types individually.

## Notes
- Only environment variables prefixed with VITE_ are exposed to the client at build time and appear under import.meta.env; reading other variables will yield undefined at runtime.
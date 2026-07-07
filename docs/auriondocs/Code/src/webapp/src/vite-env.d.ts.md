# vite-env.d

> **File:** `src/webapp/src/vite-env.d.ts`  
> **Kind:** file


This file is a TypeScript declaration that brings in the Vite client type definitions. By referencing vite/client, it exposes Vite's ambient globals (notably import.meta.env) to the TypeScript compiler, enabling proper type checking and editor support across the project without adding runtime code.

## Remarks
Because it is a compile-time reference, it has no runtime effect; it simply informs the type system about Vite-specific globals. It centralizes environment typing so all modules can rely on consistent import.meta.env shapes and Vite client APIs. This improves editor IntelliSense and helps catch misnamed environment variables at compile time.

## Example
```typescript
// Access a Vite environment variable with proper typing
console.log(import.meta.env.VITE_API_BASE_URL);
```

## Notes
- Ensure the file is included in your tsconfig.json so the types are visible to the TypeScript compiler.
- It provides only typings; there is no runtime import or side effect.
- Upgrading Vite may require reloading your editor/tsserver to pick up updated client types.

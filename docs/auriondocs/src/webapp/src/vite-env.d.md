# vite-env.d

> **File:** `src/webapp/src/vite-env.d.ts`  
> **Kind:** file

Provides a project-wide reference to Vite's built-in client typings so TypeScript recognizes Vite-specific globals (for example import.meta.env and HMR helpers). Include this file in a Vite + TypeScript project when you want the editor and compiler to pick up Vite types automatically without importing them everywhere.

## Remarks
This file is a minimal ambient declaration that pulls in the types published by Vite (the "vite/client" type package). It exists as a central, conventionally named .d.ts file so the TypeScript compiler (and IDE) will surface Vite-specific types across the codebase and so you can add project-wide augmentations (for example to ImportMetaEnv) in one place.

## Example
```typescript
/// <reference types="vite/client" />

// Augment Vite's ImportMetaEnv to add your app-specific env vars
interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_FEATURE_FLAG?: 'on' | 'off';
}

// Optionally ensure ImportMeta.env uses the above shape
interface ImportMeta {
  readonly env: ImportMetaEnv;
}
```

## Notes
- The file has no runtime effect; it only influences TypeScript's type checking and IDE completion. 
- Ensure the .d.ts file is included by your tsconfig (commonly by placing it under src/) so the compiler sees the triple-slash reference. 
- Do not import this file from application code; it is meant for global type augmentation only. 
- After editing type augmentations here you may need to restart the TypeScript server or dev server for changes to take effect.
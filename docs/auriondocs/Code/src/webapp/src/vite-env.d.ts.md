# vite-env.d

> **File:** `src/webapp/src/vite-env.d.ts`  
> **Kind:** file

Provides Vite's client type definitions to the TypeScript compiler by referencing the built-in `vite/client` declaration file. This makes Vite-specific globals (for example `import.meta.env` and HMR helpers) available project-wide without adding those types inline.

## Remarks
Keeping this small ambient declaration file lets the project pick up Vite's ambient types automatically (when the file is included by your tsconfig or sits under an included folder). It also serves as a convenient place to add project-specific augmentations to `ImportMetaEnv` if you need to declare strongly-typed runtime environment variables.

## Example
```typescript
// Read a Vite runtime variable (VITE_ prefix required for env variables to be exposed)
const apiUrl = import.meta.env.VITE_API_URL as string;
console.log('API URL:', apiUrl);
```

## Notes
- The file must have a `.d.ts` extension and be included by your tsconfig (or be located under an included path) for the ambient types to apply.
- To declare project-specific env keys, augment the `ImportMetaEnv` interface in a `.d.ts` file rather than editing `vite/client` directly.
- Vite only exposes environment variables that are prefixed with `VITE_` to the client-side code; other variables remain server-only.
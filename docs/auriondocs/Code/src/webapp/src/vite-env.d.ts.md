# vite-env.d

> **File:** `src/webapp/src/vite-env.d.ts`  
> **Kind:** file


vite-env.d.ts is a minimal ambient declaration that pulls in Vite's client typings for the TypeScript project. By referencing vite/client, it makes Vite's runtime globals and environment typings available to the compiler and IDE without emitting any runtime code.

## Remarks
These typings are centralized to ensure consistent type availability across the project without introducing runtime dependencies. It ensures that TypeScript and IDEs understand Vite-specific globals (such as import.meta.env) and related runtime behavior at development time, reducing false type errors.

## Notes
- This file is pure type information; no emitted JavaScript is produced.
- If your tsconfig.json uses a restricted 'types' array, you may need to add 'vite/client' to that list or rely on this ambient file to supply the types.
- Renaming or deleting this file can cause TypeScript to lose awareness of Vite-specific globals.
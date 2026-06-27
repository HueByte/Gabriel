# App

> **File:** `src/webapp/src/App.tsx`  
> **Kind:** function

Renders the application root by composing the global authentication provider around the router. Use this component as the top-level React component when mounting the app so that routing and route components can access authentication context and any other global providers added here.

## Remarks
This is the central composition point for cross-cutting concerns (authentication, routing and any other global providers such as theming or error boundaries). Placing AuthProvider above RouterProvider guarantees that route components, loaders, and actions can read authentication state or helpers from context. Keep changes to global initialization here to avoid spreading provider wiring across the codebase.

## Example
```typescript
import ReactDOM from 'react-dom/client';
import { App } from './App';

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(<App />);
```

## Notes
- The order of providers matters: AuthProvider must wrap RouterProvider so routes can access auth context.
- App has no props and performs no side effects itself; to change global configuration, update the providers or wrap App in a higher-level component.
- If the application needs performance optimization when re-rendering the root, consider memoizing providers or moving expensive initialization out of render.

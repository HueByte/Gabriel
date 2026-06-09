# App

> **File:** `src/webapp/src/App.tsx`  
> **Kind:** function

Top-level React component that composes an AuthProvider around the application's RouterProvider. Use this component as the root of the React tree so routing has access to authentication context and any auth-dependent route elements.

## Remarks
This component intentionally remains minimal: it centralizes two app-wide concerns — authentication context and routing — so the rest of the app can assume both are present. It does not accept props and expects the router and AuthProvider to be provided/imported from the surrounding module scope. Keeping this composition in one place simplifies the application's bootstrap (index) file and ensures consistent ordering (AuthProvider above RouterProvider).

## Example
```typescript
import React from 'react';
import { createRoot } from 'react-dom/client';
import { App } from './App';

createRoot(document.getElementById('root')!).render(<App />);
```

## Notes
- Use <App /> as a React component (JSX) rather than calling it as a plain function; it returns JSX and relies on React rendering.
- The implementation assumes `router` and [`AuthProvider`](auth/AuthContext.tsx.md) are defined and initialized in module scope; if your router or auth provider requires async setup, ensure that is completed before rendering the root App.
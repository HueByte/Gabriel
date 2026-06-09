# PublicRoute

> **File:** `src/webapp/src/routes/PublicRoute.tsx`  
> **Kind:** function

Renders a route wrapper that only allows unauthenticated (public) access. While authentication status is unresolved it shows a loading indicator; if a user is authenticated it redirects to the app root, otherwise it renders nested routes via Outlet. Use this when you want pages like sign-in or sign-up to be inaccessible to already-signed-in users.

## Remarks
This component relies on useAuth to determine authentication state and is intended to be used as the element for a parent Route in React Router v6. It centralizes the logic for guest-only pages so individual pages do not need to check auth status themselves. The redirect uses Navigate with replace to avoid leaving the public route on the browser history stack.

## Example
```typescript
// AppRoutes.tsx
import { Routes, Route } from 'react-router-dom';
import { PublicRoute } from './routes/PublicRoute';
import Login from './pages/Login';
import Register from './pages/Register';

function AppRoutes() {
  return (
    <Routes>
      <Route element={<PublicRoute />}>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
      </Route>
      {/* other routes */}
    </Routes>
  );
}
```

## Notes
- The component treats user === undefined as "still loading" — ensure useAuth uses undefined as the initial unresolved state; otherwise loading behavior may differ.
- Redirect target is hard-coded to "/"; change it if your app requires a different post-login destination.
- In SSR environments take care to avoid hydration mismatches if authentication state differs between server and client.
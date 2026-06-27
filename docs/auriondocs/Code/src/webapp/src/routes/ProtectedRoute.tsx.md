# ProtectedRoute

> **File:** `src/webapp/src/routes/ProtectedRoute.tsx`  
> **Kind:** function

Renders a route guard that enforces authentication for nested routes. Use this component when you want to block access to child routes until authentication state is known; it shows a loading placeholder while auth is resolving, redirects unauthenticated users to /login, and renders nested routes when a user is authenticated.

## Remarks
ProtectedRoute is a small router-level guard that relies on an external useAuth hook and react-router hooks. It treats useAuth().user === undefined as "auth state is loading", useAuth().user === null as "not authenticated" (redirect to login), and any other (truthy) value as an authenticated user (render the Outlet). The redirect includes the current location in the navigation state so the app can return the user to the original destination after a successful login, and uses replace to avoid leaving the protected URL in the browser history.

## Example
```typescript
// Typical react-router v6 usage — wrap protected child routes with ProtectedRoute
import { Routes, Route } from 'react-router-dom';
import ProtectedRoute from './routes/ProtectedRoute';

<Routes>
  <Route element={<ProtectedRoute />}>
    <Route path="/dashboard" element={<DashboardPage />} />
    <Route path="/settings" element={<SettingsPage />} />
  </Route>
  <Route path="/login" element={<LoginPage />} />
</Routes>
```

## Notes
- The component must be rendered inside a react-router <Router> (it uses useLocation, Navigate, and Outlet).
- Behavior depends on the contract of useAuth(): undefined = loading, null = not authenticated, non-null = authenticated. If your hook uses different sentinel values, adjust the checks accordingly.
- Redirect preserves the origin location via state.from so the login flow can navigate back after successful authentication.
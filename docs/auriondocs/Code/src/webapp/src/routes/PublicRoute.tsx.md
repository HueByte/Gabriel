# PublicRoute

> **File:** `src/webapp/src/routes/PublicRoute.tsx`  
> **Kind:** function

Renders a wrapper route for pages that should only be accessible to unauthenticated users. While the authentication status is being determined it shows a loading state; if the user is signed in it redirects to the app root, otherwise it renders nested public routes via <Outlet />.

## Remarks
This component is intended to wrap routes like sign-in, sign-up, password-reset, etc., so authenticated users are redirected away and unauthenticated users can access the nested pages. It depends on the app's useAuth hook returning undefined while loading, a truthy value for an authenticated user, and a falsy value (e.g., null) for unauthenticated.

## Example
```typescript
// React Router v6 - wrap public routes so signed-in users are redirected
import { Routes, Route } from "react-router-dom";
import PublicRoute from "./routes/PublicRoute";
import Login from "./pages/Login";
import Register from "./pages/Register";

<Routes>
  <Route element={<PublicRoute />}>
    <Route path="/login" element={<Login />} />
    <Route path="/register" element={<Register />} />
  </Route>
  // other routes...
</Routes>
```

## Notes
- Must be rendered inside a React Router context (BrowserRouter/MemoryRouter) because it uses Navigate and Outlet.
- Behaviour relies on useAuth semantics: undefined = loading, truthy = authenticated, falsy = unauthenticated; if your auth hook uses different sentinel values update this component accordingly.
- Redirect uses replace to avoid leaving the public route in history.
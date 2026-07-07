# PublicRoute

> **File:** `src/webapp/src/routes/PublicRoute.tsx`  
> **Kind:** function

```typescript
export function PublicRoute()
```


PublicRoute renders public-only routes by gating access based on authentication state. It uses the useAuth hook to obtain the current user; while the authentication status is loading (user === undefined), it shows a loading indicator; if a user is already authenticated, it redirects to the root path; otherwise, it renders the nested routes via Outlet so unauthenticated users can access the login or signup pages.

## Remarks
This component serves as the public-side guard in a React Router configuration. By centralizing the pattern of redirecting authenticated users away from auth pages, it keeps route definitions concise and consistent. It relies on React Router's Navigate for redirection and Outlet for rendering child routes, decoupling the authentication check from the page components.

## Example
```typescript
import { PublicRoute } from 'src/webapp/src/routes/PublicRoute';
import LoginPage from 'src/webapp/src/pages/LoginPage';
import RegisterPage from 'src/webapp/src/pages/RegisterPage';
import { Routes, Route } from 'react-router-dom';

<Routes>
  <Route element={<PublicRoute />}>
    <Route path="/login" element={<LoginPage />} />
    <Route path="/register" element={<RegisterPage />} />
  </Route>
</Routes>
```

## Notes
- The redirect uses replace, so authenticated users won't be able to navigate back to the public page with the back button.
- If the useAuth hook never resolves (remains undefined), the loading state will persist; ensure your auth hook reliably updates to a defined user value.
- Use PublicRoute only for pages that should be inaccessible to authenticated users (e.g., login, signup). If a page should be accessible to everyone, wrap it directly in a Route without PublicRoute.
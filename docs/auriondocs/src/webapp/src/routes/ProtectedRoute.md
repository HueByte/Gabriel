# ProtectedRoute

> **File:** `src/webapp/src/routes/ProtectedRoute.tsx`  
> **Kind:** function

Renders its child route(s) only for authenticated users. While the authentication state is unresolved the component shows a brief loading UI; if there is no authenticated user it redirects to /login and preserves the attempted location in navigation state so the app can return the user after a successful sign-in.

## Remarks
Use this as a route-level guard around routes that require an authenticated user. It centralizes the common pattern of: wait for auth to finish, redirect unauthenticated users to the login page, and allow authenticated users to proceed. The implementation depends on the contract of the useAuth hook (user === undefined means loading, user === null means unauthenticated). It uses react-router's Navigate with replace to avoid leaving the protected route on the history stack and Outlet to render nested routes.

## Example
```typescript
// In your router configuration
<Routes>
  <Route element={<ProtectedRoute />}>
    <Route path="/dashboard" element={<Dashboard />} />
    <Route path="/settings" element={<Settings />} />
  </Route>

  <Route path="/login" element={<Login />} />
</Routes>

// In Login, redirect back after successful sign-in
function Login() {
  const location = useLocation();
  const navigate = useNavigate();
  const from = (location.state as any)?.from?.pathname || '/';

  async function onSuccess() {
    // after authenticating the user
    navigate(from, { replace: true });
  }

  return <LoginForm onSuccess={onSuccess} />;
}
```

## Notes
- This component assumes useAuth returns undefined while loading and null for unauthenticated; mismatching that contract will change behavior.
- Navigate uses replace to prevent the protected route staying in history; this affects back-button behavior intentionally.
- If your app uses server-side rendering, ensure useAuth and useLocation behave correctly during hydration (this component relies on client-side routing hooks).
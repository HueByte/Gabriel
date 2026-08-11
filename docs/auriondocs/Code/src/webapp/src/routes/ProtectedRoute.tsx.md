# ProtectedRoute

> **File:** `src/webapp/src/routes/ProtectedRoute.tsx`  
> **Kind:** function

```typescript
export function ProtectedRoute()
```


ProtectedRoute is a React component that guards its nested routes by enforcing an authenticated user. It reads authentication state from the app's auth context via useAuth and preserves navigation intent with useLocation so users return to their original destination after logging in. If the auth state is still loading (user is undefined), it renders a lightweight loading placeholder; if there is no authenticated user (user === null), it redirects to /login and passes the current location in state to support post-login redirection; when a user is present, it renders the child routes via Outlet.

## Remarks
ProtectedRoute centralizes the common authentication-guard pattern for React Router, reducing duplication and aligning login-flow behavior across protected sections. It relies on Navigate for redirection and Outlet to render nested routes, keeping the routing logic declarative and easy to audit.

## Notes
- Hard-coded login path '/login' is a coupling; adapt if your app uses a different login route or a configurable redirect.
- The loading state (undefined user) is client-side and may briefly show the loading placeholder even if a session exists; ensure the auth provider properly initialises the user state to minimise flicker.
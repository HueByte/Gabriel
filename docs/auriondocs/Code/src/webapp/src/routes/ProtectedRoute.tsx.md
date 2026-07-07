# ProtectedRoute

> **File:** `src/webapp/src/routes/ProtectedRoute.tsx`  
> **Kind:** function

```typescript
export function ProtectedRoute()
```


ProtectedRoute is a small React component that guards nested routes by consulting the authentication state. It reads the current user from the authentication context via useAuth and the current location via useLocation to enable post-login redirects. When the authentication state is still loading (user is undefined), it renders a lightweight loading indicator. If the user is explicitly unauthenticated (user is null), it redirects to the login page with replace and passes the original location in state so the user can be returned after login. When a valid user is present, it renders an Outlet to render the guarded child routes. Use ProtectedRoute to wrap routes that require authentication, instead of sprinkling inline guards across pages.

## Remarks
ProtectedRoute centralizes the authentication gating for a subtree of routes, decoupling page components from routing concerns. It ensures a consistent UX for unauthenticated access by redirecting to login and preserving the destination in state. The useLocation-based state payload enables a smooth return flow after successful login, while the replace navigation avoids polluting the browser history with intermediate login steps.

## Notes
- The loading UI uses a CSS class named auth-loading; adjust styling to fit your app's theme.
- The component relies on three distinct authentication states: undefined (loading), null (not authenticated), and a user object (authenticated); if the auth hook returns a different value, behavior should be validated.
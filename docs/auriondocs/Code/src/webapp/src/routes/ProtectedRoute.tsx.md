# ProtectedRoute

> **File:** `src/webapp/src/routes/ProtectedRoute.tsx`  
> **Kind:** function

```typescript
export function ProtectedRoute()
```


ProtectedRoute is a route-guard component for React Router that ensures only authenticated users can access its nested routes. While the authentication state is determining the current user, it renders a loading indicator; if the user is not authenticated, it redirects to the login page while preserving the original destination; otherwise it renders the nested routes via Outlet.

## Remarks
ProtectedRoute centralizes authentication checks for a group of routes, so individual pages don't need to implement their own redirects. It relies on a three-state pattern from useAuth: undefined (loading), null (not authenticated), and a user object (authenticated), and it uses React Router v6's Navigate and Outlet to perform redirect and render children. Use it by wrapping a set of protected routes in the Route tree to apply the guard at the group level.

## Example
```typescript
// Example usage with React Router v6
<Routes>
  <Route element={<ProtectedRoute />}>
    <Route path="/dashboard" element={<Dashboard />} />
    <Route path="/settings" element={<Settings />} />
  </Route>
  <Route path="/login" element={<Login />} />
</Routes>
```

## Notes
- The loading state depends on useAuth returning undefined during the auth check; if you always know the user synchronously, the loading branch may render only briefly or not at all.
- The redirect to /login preserves the original location in state.from so the app can navigate back after login.
# PublicRoute

> **File:** `src/webapp/src/routes/PublicRoute.tsx`  
> **Kind:** function

```typescript
export function PublicRoute()
```


PublicRoute is a small React component that reads the current user from useAuth. It renders different outputs based on the authentication state: a loading indicator while the user is being determined, a redirect to the app root for authenticated users, or the nested routes for unauthenticated users.

## Remarks
PublicRoute centralizes the common pattern of guarding public pages (like login or signup) so you don't duplicate auth checks across routes. It ensures a consistent user experience by redirecting logged-in users away from public pages and by showing a loading state until auth resolves before rendering child routes.

## Notes
- The loading state relies on useAuth returning undefined during the check; if your auth hook signals loading differently, adjust the condition accordingly.
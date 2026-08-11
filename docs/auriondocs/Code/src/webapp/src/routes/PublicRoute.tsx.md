# PublicRoute

> **File:** `src/webapp/src/routes/PublicRoute.tsx`  
> **Kind:** function

```typescript
export function PublicRoute()
```


PublicRoute is a small React component that acts as a gate for routes intended for unauthenticated users. It reads the current user from the authentication context via useAuth and renders one of three outcomes: a loading indicator while the auth state is unresolved (user === undefined), a redirect to the home page ("/") when a user is already authenticated (user is truthy), or an Outlet to render nested routes when there is no authenticated user. This pattern centralizes the logic for public pages (e.g., login or signup) so authenticated users are consistently redirected away and unauthenticated users can access the public content.

## Remarks
PublicRoute serves as the centralized gate for routes meant for guests. It decouples the decision logic from each page by consulting the auth state and either redirecting authenticated users away or exposing the nested routes to unauthenticated visitors. It sits as the element for a route in React Router, with child routes rendered via Outlet, so you can group all public pages under this wrapper.

## Notes
- The loading state occurs while useAuth has not yet resolved; ensure your auth provider eventually sets a defined user value.
- The redirect uses replace to prevent back-navigation to the auth page after authentication; adjust the target path if your app uses a different home route.
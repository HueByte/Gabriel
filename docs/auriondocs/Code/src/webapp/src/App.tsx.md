# App

> **File:** `src/webapp/src/App.tsx`  
> **Kind:** function

```typescript
export function App()
```


App is the root React component that bootstraps the UI by wrapping the entire app in the authentication context and rendering the router. Use App when you want to initialize the app with a global AuthProvider around the Router so all routes and components can access auth state and actions.

## Remarks
By composing AuthProvider with RouterProvider, App centralizes the top-level providers and keeps the rest of the component tree focused on UI concerns. This design makes App the natural place to adjust global bootstrapping—such as adding other providers or tweaking the router configuration—while guaranteeing a consistent authentication context across the navigation graph. It acts as the single entry point for app-wide initialization and ensures components rendered via the router can rely on authentication data and actions.

## Notes
- If the provider hierarchy is altered so AuthProvider is not the outermost wrapper, descendants that depend on auth context may not receive updates.
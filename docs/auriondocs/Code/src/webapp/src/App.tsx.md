# App

> **File:** `src/webapp/src/App.tsx`  
> **Kind:** function

```typescript
export function App()
```


App is the root React component that wires the authentication context to the rest of the application by wrapping the RouterProvider in AuthProvider. Use App when bootstrapping the UI so that all routes and components can access the current user and auth helpers via the Auth context.

## Remarks
AuthProvider encapsulates authentication state and related logic, while RouterProvider manages the app's route configuration. By composing these concerns at the root, consuming components can rely on a consistent, globally available authentication state without needing to thread it through props.
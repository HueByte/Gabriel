# AuthContext.tsx

> **Source:** `src/webapp/src/auth/AuthContext.tsx`

## Contents

- [AuthState](#authstate)
- [AuthProvider](#authprovider)
- [formatError](#formaterror)
- [handler](#handler)
- [useAuth](#useauth)

---

## AuthState
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** interface

```typescript
export interface AuthState
```


AuthState defines the shape of the authentication state consumed by the app's auth context. It exposes the current user, and three asynchronous actions to modify the session: login, register, and logout. The user field expresses three meaningful states: undefined while the initial /me request is resolving, null when authentication has been definitively determined as false, or a MeResponse object when the user is authenticated.

## Remarks
This interface acts as a stable contract between UI components and the authentication implementation. It centralizes auth concerns behind a simple, strongly-typed API, enabling React context providers to update the UI based on the user state and to invoke login/register/logout without leaking implementation details. The tri-state user field is intentional: it helps prevent flicker by making the initial loading state explicit.

## Example
```typescript
// Example usage in a component that consumes AuthState
import React from 'react';
import { AuthState } from './src/webapp/src/auth/AuthContext';

function AuthDemo({ auth }: { auth: AuthState }) {
  if (auth.user === undefined) {
    return <span>Loading user...</span>;
  }

  if (auth.user === null) {
    return (
      <button onClick={() => auth.login('user@example.com', 'password')}>
        Log in
      </button>
    );
  }

  return (
    <button onClick={() => auth.logout()}>
      Log out
    </button>
  );
}
```

## Notes
- Do not treat undefined as unauthenticated; it's an in-flight state during initial resolution.
- The login/register/logout methods are asynchronous; callers should handle rejections and provide user feedback.
- The user field being null indicates a confirmed unauthenticated state; after a successful login or logout, the value toggles accordingly.

---

## AuthProvider
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function AuthProvider(
```


AuthProvider is a React functional component that acts as a context provider for authentication. It wraps its children with an authentication context, establishing and exposing the authentication state and related actions to all descendant components.

## Remarks
This abstraction centralizes authentication concerns, delivering a single source of truth for user identity and session state across the app. By providing the context at a high level, components can access auth information without prop-drilling, and it enables easier testing and mocking of authentication behavior.

## Notes
- Ensure the provider is mounted high enough in the component tree so all components that require auth can access the context.
- If the provider recreates its context value on every render, downstream consumers may re-render unnecessarily; memoize the context value to avoid this.
- When dealing with asynchronous login flows or token refresh, handle loading and unauthenticated states gracefully to prevent UI flicker.

---

## formatError
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
function formatError(e: unknown, fallback: string): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `e` | `unknown` | — |
| `fallback` | `string` | — |

**Returns:** `string`


Formats an unknown error into a stable, human-readable string for display. If the error is an ApiError, it prefers the detail field from its body when present; otherwise it uses the ApiError's message. If the error is a plain Error, it returns its message. For any other value, it returns the supplied fallback string.

## Remarks
Centralizes error-to-string logic for the UI and logs, ensuring consistent messaging across error paths and reducing boilerplate at call sites. It encapsulates ApiError-specific structure (body.detail) behind a single helper, so callers don't need to know where the detail comes from.

## Notes
- Relies on ApiError being a real runtime constructor; otherwise the ApiError branch never executes.
- The function assumes the ApiError body shape is { detail?: string; title?: string }; if it differs, detail may be undefined.
- An empty detail string will be returned as-is, which may be undesirable in some UIs.

---

## handler
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
const handler = () =>
```


handler is a no-argument arrow function that invokes logout() and discards its return value (via the void operator). When invoked, it delegates to logout() and does not return a value. Developers would reach for it as a callback to wire a UI action (for example, a button click) to trigger logout without caring about a return value.

## Remarks
It serves as a lightweight event-callback adapter that decouples a UI trigger from the underlying logout implementation by providing a ready-to-pass function reference.

---

## useAuth
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function useAuth(): AuthState
```

**Returns:** `AuthState`


useAuth is a small React hook that exposes the current authentication state from AuthContext to functional components. It reads the context via useContext and returns the AuthState value, but guards against misuse by throwing a descriptive error if it is used outside an AuthProvider. This provides a typed, provider-scoped entry point to authentication data without requiring components to reference the context directly.

---
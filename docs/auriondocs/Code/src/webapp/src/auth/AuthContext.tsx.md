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


AuthState defines the runtime shape of the authentication state exposed by the app's authentication context. It exposes the current user (undefined while the initial /me request is resolving, null when unauthenticated, or a MeResponse when authenticated) and three asynchronous actions: login, register, and logout. Use AuthState when you need to read authentication status and trigger authentication flows in a type-safe way across UI components or providers.

## Remarks
AuthState serves as a contract that decouples UI from the specifics of how authentication is performed, centralizing user state and authentication actions. The undefined sentinel for user represents the loading phase during the initial user resolution, helping avoid flicker in the first render before the /me call completes. This shape makes it straightforward to implement or consume a shared auth context without baking in implementation details into every component.

## Example
```typescript
// Minimal, valid AuthState instance demonstrating proper typing
const exampleAuthState: AuthState = {
  user: undefined,
  login: async (email: string, password: string) => {
    // perform login
  },
  register: async (email: string, password: string) => {
    // perform registration
  },
  logout: async () => {
    // perform logout
  },
};
```

## Notes
- Treat undefined as a loading state for the current user; do not assume authentication status until it becomes either null or a MeResponse.
- Distinguish between null (unauthenticated) and a populated MeResponse to drive UI states (e.g., show login form vs. show user menu).
- Since login/register/logout return Promises, callers should await these operations and handle potential errors, while the host implementation updates the user field accordingly to reflect the new authentication status.

---

## AuthProvider
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function AuthProvider(
```


AuthProvider is a React functional component that serves as the authentication context provider for the application. It accepts a children prop and renders those children within the authentication context, enabling descendant components to access authentication state and actions without prop drilling. By wrapping the app in AuthProvider, you centralize authentication logic and ensure consistent access to user data and authentication operations across the UI.

## Remarks
AuthProvider centralizes authentication concerns and is intended to be mounted near the root of the component tree. It works with a shared context to expose data such as the current authentication status and methods for signing in or signing out, so all consumers can react to auth changes without needing to pass callbacks through props. This abstraction simplifies testing and future refactors by isolating the authentication mechanism from UI components.

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


formatError converts an unknown error into a user-facing string, using a provided fallback when needed. It preferentially surfaces API-provided details for ApiError by reading body.detail, then falls back to the error's message, and finally to the fallback. If the value is a plain Error, it returns its message; otherwise, it returns the fallback.

## Remarks

This function centralizes how errors are presented in the UI, especially within authentication flows where API responses may carry structured error details. By surfacing detail when available, callers avoid exposing uncertain internal state and still provide meaningful feedback to users. The instanceof ApiError check relies on ApiError being a runtime class; if not, the function gracefully handles plain Errors and other values.

## Notes

- If you pass an ApiError with no detail and no message, the fallback is used.
- The check uses e instanceof ApiError; ensure ApiError is available in the runtime environment.
- The function only reads body.detail; other fields are ignored.

---

## handler
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
const handler = () =>
```


A tiny wrapper that delegates to logout() and discards its return value. You’d reach for it when wiring a logout action to an event handler where you neither need the return value nor to await completion, such as a simple onClick handler.

## Remarks
This symbol acts as a thin adapter between UI events and the central logout logic. By using the void operator, it guarantees the caller's interest is limited to triggering the logout side effect, not consuming any value or promise it may produce. This keeps the call site clean and explicit about ignoring the logout result, while still delegating the actual logout work to the shared implementation.

## Example
```typescript
// Example: wire up the handler to a logout button via DOM
document.querySelector('#logoutBtn')?.addEventListener('click', handler);
```

## Notes
- If logout returns a promise, not awaiting it means the logout may still be in flight after the event handler returns; consider awaiting inside the handler or handling the promise at call site.
- If logout can throw synchronously, wrap in try/catch to avoid surprising propagation from an event handler.

---

## useAuth
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function useAuth(): AuthState
```

**Returns:** `AuthState`


useAuth is a React hook that retrieves the current authentication state from AuthContext and returns it to the caller. It enforces correct usage by throwing an error if used outside an AuthProvider, ensuring you always work with a valid AuthState.

## Remarks
Provides a single typed entry point to the authentication state, abstracting away direct useContext calls. The runtime guard helps fail fast during development when a provider is missing, guiding proper app wiring. Because it uses useContext, components consuming useAuth will re-render as the AuthState changes.

## Notes
- If you call useAuth outside of an AuthProvider, you'll get an Error with message 'useAuth must be used inside <AuthProvider>'.
- The guard relies on a non-null AuthState being provided; if the context value is ever null/undefined, the hook will throw.

---
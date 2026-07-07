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


AuthState defines the authentication state and actions exposed by the app's auth context. It exposes the current user (which can be undefined while loading, null when unauthenticated, or a MeResponse when authenticated) and asynchronous methods login, register, and logout for performing authentication flows.

## Remarks
AuthState serves as a boundary between the UI and the authentication back-end. By centralizing user state and the three core operations, it prevents scattered fetches and divergent loading states across components. The MeResponse dependency shapes the user object when authenticated, while the undefined state provides a flicker-free loading signal during the initial /me call. The promise-based methods give callers a clear path to handle success, failure, and loading-driven UI changes.

## Notes
- Be mindful of the three-valued user state: undefined for loading, null for unauthenticated, MeResponse for authenticated. Components should check isLoading when user === undefined, and treat null and MeResponse appropriately.
- login/register/logout return promises; ensure to catch errors and update UI accordingly.

---

## AuthProvider
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function AuthProvider(
```


AuthProvider is a React function component that serves as the authentication context provider for the web application. It accepts a single prop, children, and renders them within the authentication context, allowing descendant components to access authentication state and related actions via the context.

## Remarks
AuthProvider sits at the boundary between the authentication data layer and the UI, centralizing auth state and actions. By providing a single, testable context, it decouples components from the underlying store and makes it easier to swap implementations or mock behavior in tests.

## Notes
- Consumers must be rendered within AuthProvider to access the authentication context; rendering outside may yield undefined values from the context.
- To minimize unnecessary re-renders, avoid changing the provider's value object on every render; memoize the context value or provide stable callbacks.

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


FormatError normalizes different error shapes into a stable, user-facing message string. It prefers ApiError.detail when available, otherwise falling back to ApiError.message or the provided fallback; for plain Error it returns the error.message, and for anything else it returns the fallback. Use this helper wherever the UI needs a consistent error string from API responses or unexpected errors, without duplicating formatting logic across components.

---

## handler
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
const handler = () =>
```


Defines a small, no-argument callback named handler that calls logout() and deliberately ignores the return value. Its signature is () => void, so the function cannot be invoked with parameters or produce a value for callers. Use this when you need a callback that triggers a side-effect (logging out) without propagating a result, such as wiring an event handler.

## Remarks
This abstraction isolates the act of logging out behind a clean void-returning callback, so you can pass handler around without exposing the logout's return type or behavior. It helps keep call sites focused on side effects rather than on the outcome of logout, and it aligns with typical callback signatures that do not consume or propagate values.

## Notes
- If logout is asynchronous (returns a Promise), this handler does not await completion; callers that rely on logout completing before continuing should handle the Promise explicitly (e.g., by returning or awaiting it).

---

## useAuth
> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

```typescript
export function useAuth(): AuthState
```

**Returns:** `AuthState`


useAuth is a small React hook that reads the authentication state from AuthContext and exposes it as an AuthState object. It should be used inside components that are descendants of an AuthProvider to access current authentication data without directly consuming the context, and it throws a descriptive error if used outside the provider to surface misconfigurations early.

## Remarks
This wrapper around useContext(AuthContext) provides a stable AuthState surface for consumers, decoupling components from the underlying context implementation. It also yields a clear runtime error when the provider is missing, guiding developers toward correct setup. Because a context value can change over time, consumers using useAuth will re-render as authentication state updates, aligning UI with the current user session.

## Notes
- Changes to the AuthContext value trigger re-renders in all components that call useAuth.
- Ensure an AuthProvider wraps the relevant portion of the component tree; using useAuth outside of a provider will throw at render time. For testing, wrap the component with a minimal test provider if needed.

---
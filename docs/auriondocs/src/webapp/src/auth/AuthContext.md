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

Represents the shape of the authentication state exposed by the app's auth provider. Use this interface when reading authentication status (the current user may be undefined, null, or a MeResponse) or when calling the async methods to log in, register, or log out.

## Remarks
This interface is the contract between an authentication provider (e.g. a React context or global store) and its consumers. It intentionally models three distinct states for the user field: undefined (initial /me call still resolving — used to avoid UI flicker), null (explicitly unauthenticated), and MeResponse (authenticated). The async methods let consumers trigger authentication flows without needing to know implementation details (network calls, token handling, etc.).

## Example
```typescript
// Typical React usage
import React, {useContext} from 'react';
import {AuthContext} from './AuthContext';

function ProfileOrSignIn() {
  const auth = useContext(AuthContext); // typed as AuthState

  if (auth.user === undefined) {
    // still resolving: show placeholder or spinner
    return <div>Loading...</div>;
  }

  if (auth.user === null) {
    // not signed in
    return <SignInForm onSubmit={async (email, pass) => await auth.login(email, pass)} />;
  }

  // authenticated
  return (
    <div>
      <p>Welcome, {auth.user.name}</p>
      <button onClick={() => auth.logout()}>Sign out</button>
    </div>
  );
}
```

## Notes
- Handle all three user states: undefined (initial loading), null (not authenticated), and MeResponse (authenticated). Failing to check undefined can cause UI flicker.
- The login/register/logout methods return `Promise<void>` and may reject on errors — callers should await and handle rejections.
- Do not mutate the user value directly; update it via the provider's methods or state mechanisms so all consumers stay consistent.

---

## AuthProvider

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

A React component function exported as AuthProvider that accepts a { children } prop and is intended to wrap a subtree of the application so that descendant components can consume authentication-related context or helpers. Use this provider to scope authentication state and make auth utilities available via context to all children.

## Remarks
This symbol represents the boundary for authentication state in the component tree: it centralizes auth-related data and exposes it to consumers (e.g., via a context and accompanying hooks). Place the provider at the appropriate root for the area of the app that needs access to auth state so descendant components can rely on the context it supplies.

## Example
```typescript
// Wrap the app (or a portion of it) so descendants can access auth context
<AuthProvider>
  <App />
</AuthProvider>

// Or wrap only a subtree that needs authentication
<AuthProvider>
  <ProtectedSection />
</AuthProvider>
```

## Notes
- The implementation body was not included in the provided source snippet; confirm the exact context value shape and any available helpers (e.g., login, logout, currentUser) before relying on them.
- Ensure the provider is mounted above any consumers; attempting to use auth context outside of this provider will typically yield undefined or fallback values depending on the implementation.

---

## formatError

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Returns a readable message for an unknown error value by preferring, in order: an ApiError's body.detail, the error's message, and finally the provided fallback string. Use this when you need a safe, displayable string from an error that may be coming from different sources (structured API errors, plain Error instances, or arbitrary values).

## Remarks
This utility centralizes error normalization for presentation or logging. It specifically recognizes an ApiError shape (extracting body.detail if present) before falling back to the standard Error.message and then to the caller-supplied fallback. The preference order ensures structured API details are surfaced when available while still providing a sensible default for unknown or non-Error values.

## Example
```typescript
// ApiError-like object with body.detail
const apiErr = /* an ApiError instance whose body has { detail: 'Invalid token' } */ null as any;
console.log(formatError(apiErr, 'Unknown error')); // -> 'Invalid token'

// Plain Error
console.log(formatError(new Error('Something went wrong'), 'Unknown error')); // -> 'Something went wrong'

// Non-error value
console.log(formatError('oops' as unknown, 'Unknown error')); // -> 'Unknown error'
```

## Notes
- The function uses instanceof ApiError, so the check only succeeds if the thrown object is an ApiError instance from the same runtime/realm; serialized errors or cross-realm objects may not be recognized.
- Only body.detail is inspected from the ApiError; body.title is not used.
- If both detail and message are absent on an ApiError, the fallback is returned.

---

## handler

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Calls the surrounding module's logout function in a fire-and-forget way; use this when you want a synchronous event handler that triggers logout without awaiting its completion.

## Remarks
The handler invokes logout and intentionally discards any returned value (the leading void). This keeps the handler synchronous and prevents an unhandled-promise-return value from being propagated to the caller. If you need to observe completion or handle errors, replace this with an async handler that awaits logout or attaches explicit .then/.catch handlers.

## Example
```typescript
// Typical use in a React component
<button onClick={handler}>Sign out</button>

// If you need to await and handle errors instead:
const handlerAsync = async () => {
  try {
    await logout();
    // post-logout work here
  } catch (err) {
    // handle logout error
  }
};
```

## Notes
- The current handler swallows any promise returned by logout; errors from logout will not be observed.
- If you need to call preventDefault or access the event, accept the event parameter (e.g. (e: React.MouseEvent) => { e.preventDefault(); void logout(); }).
- The handler itself is synchronous and returns undefined; switch to an async function when you must wait for logout to finish.

---

## useAuth

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Returns the current authentication context value from AuthContext and enforces that the hook is used inside an <AuthProvider>.

## Remarks
This is a small convenience hook that wraps React's useContext(AuthContext) and provides a clear runtime failure when the context is missing. Use it in any React component that needs access to authentication state or helpers exposed by the AuthProvider; it centralizes the provider existence check so callers don't have to perform the null/undefined guard themselves.

## Example
```typescript
function Profile() {
  const auth = useAuth();

  if (!auth.user) return <div>Please sign in</div>;
  return <div>Signed in as {auth.user.name}</div>;
}
```

## Notes
- The hook throws an Error if called outside of an <AuthProvider>; wrap your app (or component subtree) with AuthProvider to avoid that.
- Do not call this hook conditionally or outside React component function bodies (follow standard hook rules).


---
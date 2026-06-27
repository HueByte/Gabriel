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

Represents the authentication state and actions exposed by the application's auth context. Use this interface from UI code that needs to know whether a user is authenticated (and who they are) and to trigger authentication flows (login, register, logout).

## Remarks
The `user` property intentionally uses three states to make UI decisions explicit: `undefined` means the initial authentication check (such as a `/me` request) is still in progress — useful to avoid rendering a flash of unauthenticated UI; `null` means the user is confirmed unauthenticated; and a [`MeResponse`](../../../api/Gabriel.API/Contracts/Auth/MeResponse.cs.md) value means the user is authenticated. The action methods are asynchronous (returning ``Promise<void>``) so callers should await them and handle errors appropriately.

## Example
```typescript
// Typical consumer inside a React component
const { user, login, logout } = useContext(AuthContext);

if (user === undefined) {
  // initial auth check still running
  return <LoadingSpinner />;
}

if (user === null) {
  return <SignInForm onSubmit={async (email, password) => await login(email, password)} />;
}

// authenticated
return (
  <div>
    <h1>Welcome, {user.name}</h1>
    <button onClick={() => void logout()}>Sign out</button>
  </div>
);
```

## Notes
- Treat `undefined` and `null` differently: `undefined` = still resolving; `null` = confirmed unauthenticated.
- The auth methods are async and may reject on error — await them and surface errors to the user.
- Do not assume the `user` object is immutable; if you need a stable snapshot, copy required fields.

---

## AuthProvider

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

A React function component that acts as an authentication context provider for its child components. The exported AuthProvider accepts a children prop and is intended to wrap parts (or all) of the component tree that require access to authentication state or helpers.

## Remarks
Use AuthProvider near the application root (or around any subtree that needs auth state) to avoid prop drilling and to expose authentication state and actions to descendants via context. The implementation details and the exact shape of the provided context value are not available in the provided fragment; inspect the full AuthContext.tsx to learn what consumers can access.

## Example
```typescript
// Typical usage at the app root
import { AuthProvider } from './auth/AuthContext';
import App from './App';

function Root() {
  return (
    <AuthProvider>
      <App />
    </AuthProvider>
  );
}
```

## Notes
- The source fragment is incomplete (only the start of the function signature is shown). Verify the full implementation to see which context value and helpers are exposed and whether additional props are supported. 
- Ensure AuthProvider is mounted inside a React tree (i.e., within a render/JSX root); it cannot be used outside component rendering.


---

## formatError

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Converts an unknown thrown value into a deterministic, user-facing error message. Prefer this helper when rendering errors from API calls or arbitrary catch blocks so callers get a consistent string: it first returns ApiError.body.detail (when present), then ApiError.message, then Error.message for other Error instances, and finally the provided fallback string.

## Remarks
This centralizes the common pattern of unwrapping the generated ApiError shape (which may include a body with a detail field) while also handling plain Error objects and non-Error throw values. Keeping this logic in one place avoids scattering null/undefined checks and makes it easier to adjust message-selection behavior later (for example to add localization or richer formatting).

## Example
```typescript
try {
  await apiClient.doSomething();
} catch (e: unknown) {
  const message = formatError(e, 'An unexpected error occurred');
  showToast(message); // render message in UI
}
```

## Notes
- If ApiError.body.detail is present but is an empty string, that empty string will be returned (the function uses nullish coalescing (??) so only null/undefined fall through to the next option).
- Non-Error throw values (e.g. strings or plain objects) will not be inspected and will cause the fallback to be returned.
- This function does not perform localization or logging — callers should localize messages or log the original error separately if needed.


---

## handler

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Calls the module-level logout() function and deliberately discards its return value. Use this when a void-returning callback is required (for example, a React event handler) and you do not need to await the logout operation.

## Remarks
This small adapter wraps the logout call so it can be passed where a synchronous (void) callback is expected. The explicit use of the void operator indicates the author intentionally ignores any returned Promise or value from logout(), preventing the handler from becoming async.

## Example
```typescript
// Typical usage as a React onClick handler
<button onClick={handler}>Sign out</button>
```

## Notes
- If logout() returns a Promise, any rejection will be ignored here; ensure logout handles its own errors or change this handler to async and await the call.
- Because the handler does not return the logout Promise, callers cannot observe completion; switch to `const handler = async () => { await logout(); }` when callers must await or react to the result.

---

## useAuth

> **File:** `src/webapp/src/auth/AuthContext.tsx`  
> **Kind:** function

Retrieves the current authentication state from the AuthContext and enforces that the hook is used inside an <AuthProvider>. Reach for this hook from React function components or other hooks when you need access to the authenticated user, tokens, or auth helper functions instead of interacting with the context object directly.

## Remarks
This small ergonomic wrapper centralizes the useContext call and the provider boundary check so callers receive a guaranteed AuthState (or a clear runtime error). By throwing when the context is missing, components do not need to handle a nullable return value and the application surface for incorrect usage is explicit.

## Example
```typescript
function UserBadge() {
  const auth = useAuth();

  if (!auth.user) {
    return <button onClick={auth.signIn}>Sign in</button>;
  }

  return (
    <div>
      {auth.user.name}
      <button onClick={auth.signOut}>Sign out</button>
    </div>
  );
}

// Wrap your app at a top level:
// <AuthProvider>
//   <App />
// </AuthProvider>
```

## Notes
- Calling this hook outside of an <AuthProvider> will throw an Error; ensure the component tree is wrapped by the provider.
- Intended for use in React function components and other hooks — not usable in class component lifecycle methods.
- The hook returns an AuthState directly (no nullable value) because it enforces the provider presence at call time.

---
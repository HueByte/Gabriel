# authRefresh.ts

> **Source:** `src/webapp/src/api/authRefresh.ts`

## Contents

- [postRefresh](#postrefresh)
- [refreshSession](#refreshsession)
- [signalSessionExpired](#signalsessionexpired)

---

## postRefresh

> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

Sends a POST to the server endpoint that attempts to refresh the authentication session. The request includes credentials (cookies) and a JSON body containing an empty refreshToken field — the empty field is deliberate to satisfy the server's model validation so the server-side code can fall back to reading a refresh token from an HttpOnly cookie. Resolves to true when the HTTP response has a successful status (res.ok); returns false on network errors or when the response is not successful.

## Remarks
This function is a minimal client-side trigger for the server-side refresh flow: it does not parse or return tokens. The server is expected to perform the actual token rotation (typically by reading a refresh token from an HttpOnly cookie and issuing new cookies or response state). The empty refreshToken property exists solely to satisfy the server's non-nullable request model so the cookie-based fallback runs.

## Example
```typescript
// attempt to silently refresh the session; if it fails, redirect to login
const ok = await postRefresh();
if (!ok) {
  window.location.href = '/login';
}
```

## Notes
- The function swallows exceptions and returns false on errors — it never throws.
- The request relies on credentials (cookies) being sent; for cross-origin scenarios the server must allow credentials and CORS accordingly.
- The function returns only a boolean success indicator; any tokens set by the server are handled via cookies or separate endpoints and are not exposed here.

---

## refreshSession

> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

Starts or returns an in-progress session refresh operation. If a refresh is already running, subsequent calls return the same `Promise<boolean>` instead of creating a new network request. Use this when you want to avoid parallel refresh requests (single-flight behavior) and share the result with multiple callers.

## Remarks
This function is a lightweight single-flight wrapper around the underlying postRefresh() call. It stores a module-scoped Promise (refreshing) while a refresh is in progress and clears that state in a finally handler so a later call will start a new refresh. This prevents duplicate refresh requests (for example, simultaneous token refresh attempts) while keeping retry behavior simple: failures clear the in-flight marker so the next call will attempt again.

## Example
```typescript
// Multiple callers during an in-progress refresh get the same Promise
const p1 = refreshSession();
const p2 = refreshSession();
console.log(p1 === p2); // true

// Typical usage
try {
  const ok = await refreshSession();
  if (ok) {
    // session refreshed successfully
  } else {
    // refresh completed but indicated failure (depends on postRefresh semantics)
  }
} catch (err) {
  // handle network/error case
}
```

## Notes
- If postRefresh() rejects, the returned Promise rejects and the internal marker is cleared; subsequent calls will trigger a new refresh attempt.
- There is no cancellation support: once started, the refresh will run to completion and all callers share its outcome.
- The single-flight state is module-scoped; in environments with multiple module instances (e.g., different worker contexts or server processes) each instance manages its own in-flight flag.
- Callers must handle Promise rejections; the function propagates errors from postRefresh().

---

## signalSessionExpired

> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

Dispatches a plain DOM Event named by SESSION_EXPIRED_EVENT on the global window object. Use this function when the application detects an expired user session and needs to notify any subscribers (UI components, services, or other modules) without creating direct dependencies.

## Remarks
This function centralizes the "session expired" notification as a window-level event so multiple, independently authored parts of the app can react without importing the auth module or calling into it directly. It emits a plain Event (no payload), keeping the signal lightweight and decoupled from any particular handler implementation.

## Example
```typescript
// Subscribe somewhere in your application startup or a component
window.addEventListener(SESSION_EXPIRED_EVENT, () => {
  // e.g. show a login modal, clear sensitive state, redirect
  showLoginModal();
});

// Trigger the notification when you detect session expiry
signalSessionExpired();
```

## Notes
- The dispatched Event has no payload — listeners cannot read extra data from the event object.
- The Event created with new Event(...) is not cancelable and does not bubble by default.
- Dispatching is synchronous: listeners run immediately during the dispatch call.

---
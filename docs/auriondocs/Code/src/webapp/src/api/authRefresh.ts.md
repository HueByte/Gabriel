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

```typescript
async function postRefresh(): Promise<boolean>
```

**Returns:** `Promise<boolean>`


Calls the server-side refresh endpoint to re-establish authentication by sending an empty refreshToken in the request body while including credentials. The function returns true when the HTTP response indicates success (res.ok) and false if the network call fails or an exception occurs. It relies on the server reading the actual token from an HttpOnly cookie; the body field exists solely to satisfy model validation on the controller, which requires a non-nullable string member in FromBody.

## Remarks
postRefresh encapsulates a cookie-based refresh-token flow behind a simple boolean API. It keeps callers from dealing with cookie management or API-controller validation intricacies, and centralizes the edge case where the refresh token lives in a server-side HttpOnly cookie. A successful refresh results in a truthy return value; a failure (due to missing cookie, network error, or server-side rejection) yields false, allowing the caller to decide whether to prompt re-authentication or take alternative action.

## Example
```typescript
// Most common usage: attempt to refresh session and branch accordingly
const ok = await postRefresh();
if (ok) {
  // refresh succeeded, continue user flow
} else {
  // refresh failed — prompt user to re-authenticate or show login
}
```

## Notes
- The function swallows exceptions and returns false on error; callers should not rely on exceptions for flow control.
- It relies on credentials being included (credentials: 'include'); if cookies are blocked or cross-site requests are restricted, refresh may fail.
- The body must include the refreshToken field, even if empty, because the server-side model requires a non-nullable string in the body; omitting or omitting the field can cause a 400 before the cookie-based fallback is considered.

---

## refreshSession
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function refreshSession(): Promise<boolean>
```

**Returns:** `Promise<boolean>`


refreshSession ensures only a single session refresh is in-flight at a time and returns a `Promise<boolean>` for the refresh result. Use it when you need to refresh authentication without spawning multiple concurrent refresh requests; if a refresh is already in progress, the function reuses the existing promise.

## Remarks
Centralizes refresh logic to prevent duplicate network calls and race conditions during token renewal. It relies on a shared refreshing cache and a postRefresh() helper; the in-flight promise is cleared in a finally block, allowing subsequent refreshes to be started after completion. This pattern is useful anywhere multiple components may need a fresh session without coordinating refreshes themselves.

## Example
```typescript
// Only one actual refresh will be performed even if called from multiple places
async function ensureSession() {
  const ok = await refreshSession();
  if (!ok) {
    throw new Error("Session refresh failed");
  }
  // proceed with authenticated work
}

// Concurrent usage example
const a = refreshSession();
const b = refreshSession();
const [okA, okB] = await Promise.all([a, b]);
```

## Notes
- If postRefresh rejects, the returned promise rejects, but the finally block clears the in-flight flag so future calls can retry.
- The in-flight cache (refreshing) must be initialized (to null/undefined) so the first call can start a refresh.
- Do not rely on the value of postRefresh beyond the boolean it returns; refreshSession merely propagates that result to callers and manages concurrency.


---

## signalSessionExpired
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function signalSessionExpired(): void
```

**Returns:** `void`


Dispatches a browser Event signaling that the current user session has expired. Call this function when your authentication logic determines the token is no longer valid, so that any part of the app listening for SESSION_EXPIRED_EVENT can respond in a centralized way (e.g., redirecting to login, clearing user data, or prompting a token refresh). It provides a single, observable signal and hides the exact event name behind a small API surface, reducing duplication of window.dispatchEvent calls across the codebase.

## Remarks
This helper encapsulates cross-cutting session-expiration signaling. By emitting a window-level Event, it enables decoupled subscribers to react without the signaling code needing to know about who handles the expiration. Tests can spy on window.dispatchEvent, and consumers can attach listeners anywhere in the app that has access to the window object.

## Example
```typescript
// Trigger a global session-expired signal
signalSessionExpired();
```

## Notes
- The event is a plain Event with type SESSION_EXPIRED_EVENT and carries no payload by design.
- Listeners should register via window.addEventListener(SESSION_EXPIRED_EVENT, handler) before the signal is dispatched; remember to remove listeners to avoid leaks.
- If you need to convey extra information about the expiration, consider using a CustomEvent instead of Event.

---
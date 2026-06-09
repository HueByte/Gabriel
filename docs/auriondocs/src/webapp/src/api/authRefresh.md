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

Sends a POST request to the server's /api/auth/refresh endpoint to trigger a token refresh. Returns true when the HTTP response has a successful status (res.ok); false is returned for network errors or non-success responses. Use this when the client needs to refresh authentication state without direct user interaction.

## Remarks
The request includes credentials (cookies) via `credentials: 'include'` so that an HttpOnly refresh token cookie will be sent to the server. The JSON body deliberately contains a `refreshToken` property with an empty string — the server-side action expects a non-nullable `refreshToken` member and API controller model validation will reject an empty object (`{}`) before the controller code can fall back to reading the cookie. Including the field (any value) allows validation to pass and lets the server pick up the cookie.

## Example
```typescript
// Attempt to refresh tokens; if it fails, redirect to login
const ok = await postRefresh();
if (ok) {
  // refreshed; continue using the app
} else {
  // failed to refresh - clear state / navigate to sign-in
}
```

## Notes
- Returns false for network errors and for any non-2xx HTTP response (res.ok === false).
- Because `credentials: 'include'` is used, cross-origin requests require the server to allow credentials (Access-Control-Allow-Credentials: true) and to return an appropriate Access-Control-Allow-Origin.
- The function never throws; callers can rely on a boolean success value instead of handling exceptions.

---

## refreshSession

> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function refreshSession(): Promise<boolean>
```

**Returns:** ``Promise<boolean>``


Returns a `Promise<boolean>` that resolves with the result of an authentication/session refresh. If a refresh is already in progress, callers receive the same in-flight Promise instead of starting a new network request — use this to prevent duplicate refresh requests when multiple parts of the app trigger a refresh concurrently.

## Remarks
This function relies on a module-scoped variable (refreshing) to hold the current in-flight Promise and on postRefresh() to perform the actual refresh work. It uses the nullish-assignment (??=) operator to only start a new refresh when there is no existing Promise, and it resets the module state to allow future refresh attempts once the current one settles (via finally).

## Example
```typescript
// Two callers that request a refresh at the same time will share the same Promise
const p1 = refreshSession();
const p2 = refreshSession();
console.log(p1 === p2); // true

// Awaiting either resolves/rejects with the result of postRefresh()
await p1; // boolean
await p2; // same boolean
```

## Notes
- The shared variable refreshing must be a module-level `Promise<boolean>` | null/undefined; its scope determines how deduplication works (it is not cross-worker/process).
- If postRefresh() rejects, the shared Promise will reject and refreshing is cleared in finally so subsequent calls can retry.
- All callers receive the same Promise and same outcome; there is no per-caller isolation or cancellation for individual callers.

---

## signalSessionExpired

> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

Dispatches a global DOM Event named by SESSION_EXPIRED_EVENT on window to notify any interested listeners that the current user session has expired. Use this when the application detects an authentication/session timeout and needs to broadcast that fact to decoupled UI or logic (for example, to show a sign-in dialog or redirect to the login page).

## Remarks
This function centralizes the act of signaling a session-expiration so producers of that event don't need to repeat the event construction or the event name. Consumers can listen for the same event name on window and react without direct coupling to the code that detected the expiration.

## Example
```typescript
// Register a listener somewhere in your app (startup or relevant component)
window.addEventListener(SESSION_EXPIRED_EVENT, () => {
  // e.g. show login modal or redirect
  showLoginModal();
});

// When session expiration is detected (e.g. refresh token fails), call:
signalSessionExpired();
```

## Notes
- This uses the global window object: it will throw or be unavailable in non-browser (SSR) environments without a window guard.
- The function dispatches a plain Event (no detail/payload). If consumers need data about the expiration, switch to CustomEvent and include a payload.
- Repeated calls produce repeated events; listeners should handle potential duplicates if that matters.

---
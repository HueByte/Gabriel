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


The function initiates a server-side refresh of the user’s authentication state by sending a POST request to the backend refresh endpoint. It relies on the server’s cookie-based refresh flow: if an HttpOnly refresh token cookie is present, the server reads it from the cookie; otherwise it falls back to the value of body.refreshToken. To satisfy the server’s binding rules, the body always includes a refreshToken field (even when empty), which allows the controller’s non-nullable string member to pass validation and enables the cookie-based path to be taken. The call returns true when the HTTP response is OK, and false if the request fails or the server responds with an error status. It does not inspect or parse the response payload. 

## Remarks
Isolates the refresh handshake behind a small, reusable helper so the front end consistently triggers the server’s refresh logic without duplicating cookie handling or binding concerns. The implementation guarantees a non-null refreshToken field in the request body to satisfy model binding, while still preferring the HttpOnly cookie when available. This centralization reduces drift between clients and ensures a predictable refresh flow across the app. 

## Example
```ts
// Typical usage: attempt a refresh and react based on the result
if (await postRefresh()) {
  // Refresh succeeded; continue with authenticated actions
} else {
  // Refresh failed; redirect to login or show an error
}
```

## Notes
- The function returns a simple boolean: true for a successful, OK HTTP response; false for network errors or non-OK responses. It does not throw on failure and does not expose server-side error details. 
- Because credentials: 'include' is used, the request will carry cookies (e.g., HttpOnly refresh tokens). Ensure the server is configured to receive and validate cookies appropriately, and that CORS/privacy settings allow credentialed requests when necessary.

---

## refreshSession
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function refreshSession(): Promise<boolean>
```

**Returns:** `Promise<boolean>`


refreshSession ensures a single in-flight refresh; if a refresh is already in progress, callers obtain the same Promise instead of starting another, and once it completes the internal cache is cleared so future calls may refresh again.

## Remarks
Conceptually, this function acts as a tiny concurrency gate around the refresh operation. It consolidates multiple refresh requests into a single network call and exposes a simple boolean result to callers. The finally handler resets the cache regardless of success or failure, allowing new refresh attempts later. This abstraction hides boilerplate of managing a shared refresh promise and reduces race conditions around re-authentication flows.

## Example
```typescript
// Two callers trigger a refresh simultaneously; only one postRefresh runs.
const a = refreshSession();
const b = refreshSession();
const [resA, resB] = await Promise.all([a, b]);
```

## Notes
- The internal cache only stores the in-flight Promise; after completion, refreshing becomes null again, so subsequent calls may trigger a fresh refresh instead of reusing the previous result.

---

## signalSessionExpired
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function signalSessionExpired(): void
```

**Returns:** `void`


Dispatches a global SESSION_EXPIRED_EVENT on the window to signal that the current user session has expired. Use this helper when your authentication flow detects expiry and you want to notify any interested UI components in a decoupled way without wiring them directly to the refresh logic.

## Remarks
This symbol acts as a lightweight broadcast mechanism for session-expiry state. By emitting a window Event, it enables disparate parts of the UI to react (e.g., redirect to login, clear tokens) without requiring tight coupling to the refresh code. Listeners subscribe via window.addEventListener(SESSION_EXPIRED_EVENT, handler). Note: The Event is a plain Event with no payload; if you need to pass extra data, consider using CustomEvent with a detail payload.

## Notes
- This uses a browser-global Event; in non-browser environments, guard with a check for window.
- If you need to pass data with the signal, switch to CustomEvent and include a detail object.

---
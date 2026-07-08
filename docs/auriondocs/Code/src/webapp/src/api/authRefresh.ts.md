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


postRefresh is an asynchronous helper that triggers the server-side refresh-token flow by sending a POST to /api/auth/refresh with credentials included. It transmits a JSON body containing refreshToken: '' to satisfy the server's non-nullable body model while allowing the server to read the actual token from an HttpOnly cookie if present; the function resolves to true when the response status is OK (res.ok), and false if a network error occurs or the response is not OK.

## Remarks
This function centralizes the client-side refresh behavior, exposing a simple boolean success signal to callers while hiding the HTTP details. It relies on the server’s cookie-based refresh flow: the HttpOnly cookie supplies the refresh token when available, and the body field is present merely to satisfy server-side validation when the cookie is used. By returning a boolean, callers can react to refresh success or failure without parsing response payloads.

## Notes
- The request depends on cookies being available and the server allowing credentials; if cookies are blocked or CORS credentials are not permitted, the refresh will fail.
- The function swallows exceptions and returns false on error, so a false result should be interpreted as the refresh not yielding a new token.
- This helper is tightly coupled to the endpoint /api/auth/refresh and the server’s validation model; changes to the endpoint, cookie policy, or body shape could require corresponding adjustments to this code.

---

## refreshSession
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function refreshSession(): Promise<boolean>
```

**Returns:** `Promise<boolean>`


refreshSession returns a `Promise<boolean>` and coordinates session refreshes so that a single in-flight refresh is shared among all callers. If a refresh is already in progress, subsequent calls reuse the same Promise; once the operation completes, the cache is cleared to allow future refreshes.

## Remarks
This abstraction prevents multiple parallel requests to refresh the session, reducing redundant network calls and race conditions. It relies on a shared `refreshing` Promise and a final cleanup to reset the state, so callers only need to await the result rather than track concurrency themselves. The boolean result reflects the outcome from `postRefresh`, which this wrapper preserves for callers.

## Example
```typescript
async function testRefreshCoalescing() {
  const p1 = refreshSession();
  const p2 = refreshSession();
  const [r1, r2] = await Promise.all([p1, p2]);
  // r1 and r2 are the same boolean outcome from the single refresh operation
}
```

## Notes
- This function relies on nullish coalescing assignment (??=) and Promise.finally; ensure your environment targets ES2021 or newer.
- If `postRefresh` rejects, the `finally` cleanup still clears the `refreshing` cache, allowing a retry on subsequent calls.
- The boolean return value comes from `postRefresh`; if you need a specific interpretation, rely on its contract rather than assuming a particular meaning of `true`/`false` here.

---

## signalSessionExpired
> **File:** `src/webapp/src/api/authRefresh.ts`  
> **Kind:** function

```typescript
export function signalSessionExpired(): void
```

**Returns:** `void`


Signals that the user session has expired by dispatching a DOM Event named by SESSION_EXPIRED_EVENT. Use this function from your authentication/refresh flow when expiry is detected to notify any listeners in a decoupled way (e.g., redirect to login or prompt for re-authentication) without tying those reactions to the expiry-detection logic.

## Remarks

By isolating the event emission behind a single function, the codebase gains a single source of truth for signaling expiry. Listeners can subscribe to the SESSION_EXPIRED_EVENT on the global window to react in a centralized, decoupled fashion.

## Notes

- Relies on the browser's window and DOM Event API; in non-browser runtimes (e.g., server-side rendering or tests with a non-browser environment), window may be undefined. Guard or mock window in such contexts if needed.
- The emitted Event carries no payload; it is a simple, signal-only notification.

---
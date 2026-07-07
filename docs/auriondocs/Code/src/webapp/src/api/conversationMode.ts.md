# conversationMode.ts

> **Source:** `src/webapp/src/api/conversationMode.ts`

## Contents

- [GabrielMode](#gabrielmode)
- [setConversationMode](#setconversationmode)
- [withRefresh](#withrefresh)

---

## GabrielMode
> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** type alias

```typescript
export type GabrielMode = 'chatty' | 'elaborative' | 'concise' | 'tutor' | 'critic'
```


GabrielMode is a TypeScript type alias that enumerates the allowed conversational stances Gabriel can adopt: 'chatty', 'elaborative', 'concise', 'tutor', and 'critic'. By constraining values to this finite set, downstream code can switch Gabriel's behavior in a type-safe way without relying on free-form strings. Use GabrielMode whenever you need to specify or convey Gabriel's desired response style in configuration objects, UI selections, or API inputs, ensuring consistency across the codebase.

## Remarks
GabrielMode serves as a domain-level contract between the UI layer (mode selectors) and the conversational engine. It centralizes the allowed modes, making it easy to add or remove styles in one place and propagate changes consistently. This abstraction prevents scattered string literals and reduces the risk of invalid styles being used.

## Notes
- This is a type alias and has no runtime representation. If you need runtime constants, define a string enum or a set of constants that mirror these values.
- When parsing external input (e.g., JSON from an API), validate that the value is one of the allowed GabrielMode options to avoid downstream errors.

---

## setConversationMode
> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

```typescript
export async function setConversationMode(
  conversationId: string,
  mode: GabrielMode | null,
  signal?: AbortSignal,
): Promise<void>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `conversationId` | `string` | — |
| `mode` | `GabrielMode | null` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<void>`


Updates the mode for a conversation by issuing a PUT request to the backend at /api/conversations/{conversationId}/mode with a JSON body { mode }. It accepts either a GabrielMode value or null (to clear the mode) and returns a `Promise<void>` that resolves when the operation completes. The request is wrapped by withRefresh to automatically refresh authentication tokens as needed, and it supports cancellation via an optional AbortSignal. If the server responds with a non-ok status, it surfaces the status together with any response text as an Error.

## Remarks
This function centralizes the server mutation for conversation mode, decoupling UI code from the HTTP details and token management. By delegating to withRefresh, it ensures that authentication concerns are handled consistently across API mutations. The endpoint relies on standard browser fetch semantics with credentials: 'include' and a URL-encoded conversationId to build a stable, authenticated request.

## Example
```ts
async function clearModeForConversation(convId: string) {
  await setConversationMode(convId, null);
}
```

## Notes
- If you supply an AbortSignal, the caller can cancel the request; ensure proper cancellation handling at the call site.
- The request uses credentials: 'include'; it relies on cookies or other browser-auth mechanisms being available.
- Error messages include server-provided text when available; if the server returns empty text, the error will still report the HTTP status.

---

## withRefresh
> **File:** `src/webapp/src/api/conversationMode.ts`  
> **Kind:** function

```typescript
async function withRefresh(doFetch: () => Promise<Response>): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `doFetch` | `() => Promise<Response>` | — |

**Returns:** `Promise<Response>`


withRefresh is a small helper that encapsulates the common pattern of refreshing an authenticated session when a request returns 401. It accepts a doFetch function that performs the actual fetch and returns a Response. When called, it awaits the initial response; if the status is 401, it attempts to refresh the session via refreshSession. If the refresh succeeds, it retries the original doFetch once and returns the retried response. If the retried response remains 401, it signals that the session has expired and throws an error guiding the user to sign in again. Call this helper to centralize retry-on-unauthorized logic and keep API-call sites free from boilerplate.

## Remarks
Centralizes the refresh-on-unauthorized workflow, reducing duplication across API calls that require authentication. It isolates session-refresh semantics behind a single utility, so callers only provide the fetch logic and let withRefresh decide when to refresh. It relies on refreshSession to determine if a token refresh occurred and on signalSessionExpired to trigger the sign-out flow when refresh fails.

## Notes
- Only a single retry is attempted; there is no looping or exponential backoff. If the retried request after a refresh still returns 401, the session is considered expired.
- Concurrency caveat: if multiple requests hit 401 at once, each may trigger its own refresh; consider higher-level coordination if this is a concern.

---
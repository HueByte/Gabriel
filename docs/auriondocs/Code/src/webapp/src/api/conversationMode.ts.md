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


GabrielMode defines the set of tonal and depth configurations that Gabriel can adopt when generating responses. It restricts values to the five modes: 'chatty', 'elaborative', 'concise', 'tutor', and 'critic'. Use GabrielMode anywhere you configure Gabriel's behavior (UI mode selectors, user preferences, or response pipelines) to ensure only valid modes are chosen. This type alias encodes domain vocabulary into the type system, enabling better autocompletion, compile-time safety, and clearer intent when wiring components together.

## Remarks
GabrielMode acts as a contract between the presentation layer and the response generator. By enumerating modes as a distinct type, the system can exhaustively handle each mode in switches or mapping logic, and future extensions can add new modes with minimal ripple. Because it's a type-level construct, the actual runtime value is just a string; validation may be required if values originate from external sources.

## Example
```typescript
function configureGabriel(mode: GabrielMode) {
  // implementation applies the selected conversational style
  // e.g., set response verbosity, examples, and pacing
  applyModeToEngine(mode);
}

const mode: GabrielMode = 'elaborative';
configureGabriel(mode);
```

## Notes
- Runtime values are plain strings; if you parse mode from JSON or external input, validate against GabrielMode to avoid invalid states.


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


Updates the mode of a specific conversation by issuing an HTTP PUT request to the server endpoint for that conversation. It accepts a conversationId, a mode value of type GabrielMode or null, and an optional AbortSignal to cancel the request. The request is prepared with a JSON body containing { mode } and is sent with credentials included. The operation delegates to withRefresh to ensure a fresh authentication state before making the call. If the server responds with a non-OK status, the response text (if any) is read and included in the thrown Error to aid debugging.

On success, the function resolves to void. On failure, it throws an Error whose message includes the HTTP status and any response text returned by the server.

## Remarks
Encapsulates a small API call boundary between UI/business logic and the backend. Centralizes token refreshing, request formatting, and error handling so call sites stay simple and consistent. If you later change how conversations’ modes are updated (e.g., adding retry logic or switching to a different transport), this is the single place to adjust.

## Notes
- The error message includes the response status and body text; if the body is unavailable, the text falls back to an empty string.
- Credentials: 'include' means cookies are sent; ensure CORS allows credentialed requests.
- mode can be null; server must handle null accordingly.

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


withRefresh is a small helper that augments a fetch-like operation with automatic session refresh logic. It runs the provided doFetch function to obtain a Response; if the server replies with 401 Unauthorized, it attempts a single session refresh and retries the fetch. If the retried request still yields 401, it signals that the session has expired and throws an error prompting the user to sign in again. The caller receives the final Response from either the initial call or the retried call.

## Remarks
Centralizes the authentication-handling pattern for API calls that require a valid session, reducing duplicated refresh-and-retry boilerplate. It coordinates with refreshSession and signalSessionExpired to deliver a consistent user experience when a session has expired, without forcing callers to manage refresh flows directly.

## Example
```typescript
// Common usage: wrap a fetch-like operation so it will refresh auth if needed
const response = await withRefresh(() => fetch('/api/messages', { credentials: 'include' }));
```

## Notes
- It performs at most one refresh and one retry after a 401.
- If, after the optional retry, the response is still 401, the function signals session expiry and throws a standard error.
- Non-401 responses pass through unchanged; this function does not alter successful responses or other error cases.

---
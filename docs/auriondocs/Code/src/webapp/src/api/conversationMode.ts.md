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


GabrielMode is a string literal union that defines the set of tonal styles Gabriel can adopt in its responses. Use GabrielMode to steer the bot's tone in a centralized way (e.g., 'concise' for brief answers or 'tutor' for guided instruction) rather than scattering tone-selection logic across components.

## Remarks
GabrielMode acts as a contract between the UI and the response generator, enabling consistent behavior across parts of the system that render, log, or test Gabriel's replies. It makes it straightforward to add, remove, or rename modes in one place and have the rest of the system adapt via a switch or pattern-match. If the set of modes evolves, ensure the mode-handling paths are updated to reflect the new semantics.

## Notes
- This is a TypeScript compile-time construct; at runtime there is no automatic enforcement of the union. If you receive input from users or external APIs, validate that the string matches one of the allowed literals before treating it as GabrielMode.
- Because GabrielMode is a union of literals, invalid assignments will be rejected by the TypeScript compiler. When values come from external data, always perform a guard/validation before using them as GabrielMode.

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


## Source Code

The setConversationMode function centralizes the HTTP interaction required to update a conversation's mode. It performs a PUT request to the backend with the mode payload and relies on a refresh wrapper to handle authentication concerns, returning a resolved promise on success or throwing a descriptive error when the server responds with a non-OK status. Call this helper when you need to adjust a conversation's mode in a single, consistent API surface, rather than composing fetch calls in multiple places. Note that passing null for mode sends { mode: null } to the server, which in turn must interpret that as a clear or reset action.

```typescript
export async function setConversationMode(
  conversationId: string,
  mode: GabrielMode | null,
  signal?: AbortSignal,
): Promise<void> {
  const response = await withRefresh(
    () => fetch(`/api/conversations/${encodeURIComponent(conversationId)}/mode`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ mode }),
      signal,
    }),
  );
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`Set conversation mode failed: ${response.status} ${text}`);
  }
}
```

## Dependencies

- GabrielMode
- AbortSignal
- Promise
- JSON
- Error

## Dependency APIs (verified signatures)

The REAL, parser-verified API surface of this symbol's collaborators:

- enum `GabrielMode` (`src/api/Gabriel.Core/Entities/GabrielMode.cs`)
- AbortSignal (global DOM interface)
- Promise (global)
- JSON (global)
- Error (global)

## Symbol To Document
- Name: `setConversationMode`
- Kind: function
- File: `src/webapp/src/api/conversationMode.ts`
- Language: typescript
- ID: e8b4a552-936f-47ba-b3b9-b91f385803cf

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


withRefresh orchestrates a fetch-like operation and transparently handles an expired session by attempting a refresh and retrying once. If the retry still yields 401, it signals session expiry and throws an error instructing the user to sign in again.

## Remarks
Centralizes session refresh and retry logic for authenticated API calls. It decouples the refresh mechanics (refreshSession) from call-sites and provides a single, consistent error path when authentication cannot be restored.

## Example
```typescript
// Basic usage: auto-refresh on 401
const resp = await withRefresh(() => fetch('/api/conversations'));
```

## Notes
- It only handles the 401 path; the code will not retry on other statuses or errors from doFetch, and a thrown error from doFetch propagates.
- The second request is attempted only if refreshSession resolves to a truthy value. If the second call still results in 401, a session-expired error is thrown to prompt re-authentication.

---
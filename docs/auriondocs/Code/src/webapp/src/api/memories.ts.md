# memories.ts

> **Source:** `src/webapp/src/api/memories.ts`

## Contents

- [MemoryDto](#memorydto)
- [SaveMemoryRequest](#savememoryrequest)
- [MemoryScope](#memoryscope)
- [MemoryType](#memorytype)
- [deleteMemory](#deletememory)
- [listMemories](#listmemories)
- [saveMemory](#savememory)
- [urlFor](#urlfor)
- [withRefresh](#withrefresh)

---

## MemoryDto

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

Represents the shape of a "memory" returned from or sent to the server API. Use this DTO when reading or writing memory records across the network or when mapping API responses into client-side models; it describes the transport form (strings for timestamps and IDs, nullable project association) rather than any rich domain behavior.

## Remarks
This interface is a plain data contract between client and server: it focuses on serializable fields only. It separates the API transfer shape from any richer domain objects you might use in the client (for example, converting ISO timestamp strings to Date objects or enriching the body with parsed content). The MemoryType field identifies the category of the memory and is defined elsewhere.

## Example
```typescript
const example: MemoryDto = {
  id: "m_12345",
  projectId: null, // null = user-scope
  type: /* MemoryType value */ "note",
  name: "Shopping list",
  description: "Groceries to buy",
  body: "- Milk\n- Eggs\n- Bread",
  createdAt: "2024-06-01T12:34:56.000Z",
  updatedAt: "2024-06-02T09:00:00.000Z",
};
```

## Notes
- projectId may be null — treat null as a user-scoped memory rather than belonging to a project. 
- createdAt and updatedAt are ISO 8601 strings; convert to Date objects on the client if you need date arithmetic or formatting. 
- The DTO uses plain strings for id and timestamps; it carries no methods or behavior and should be mapped to richer models if immutability or validation is required.

---

## SaveMemoryRequest

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

```typescript
export interface SaveMemoryRequest
```


Represents the payload sent from the web UI to the API when creating or updating a memory. Use this interface whenever you need to construct the request body for the endpoint that persists a memory entry — it groups the optional project association, the memory's type, a human-readable name and description, and the memory content itself.

## Remarks
This is a plain data-transfer shape (compile-time only in TypeScript) that defines the contract between the frontend and the backend for saving memories. projectId is nullable to indicate the memory may be unassociated with any specific project; MemoryType categorizes the memory and is defined separately.

## Example
```typescript
import { MemoryType } from './memory-types';

const req: SaveMemoryRequest = {
  projectId: null, // not tied to a project
  type: MemoryType.Note,
  name: 'Meeting notes — 2025-06-01',
  description: 'Summary of architecture discussion',
  body: 'Decisions: use X for Y; follow-up: assign Z.'
};

// then send `req` to the API client that handles saving memories
apiClient.saveMemory(req);
```

## Notes
- projectId being null means "no project association"; do not use an empty string to represent that unless the API explicitly expects it.
- This interface provides compile-time guarantees only — perform any required runtime validation (length limits, allowed characters, etc.) before sending to the server.
- Ensure MemoryType value is valid for the server-side implementation; mismatched or unknown types may be rejected by the API.

---

## MemoryScope

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type

Represents the scope of a memory as a discriminated union: at minimum there are two variants indicated by the kind property — 'user' for user-scoped memories and 'project' for project-scoped memories. Use this type wherever code needs to branch on whether a memory belongs to the current user or to a project.

## Remarks
This is a simple discriminated-union (tagged union) that enables safe, idiomatic narrowing in TypeScript by switching on the kind field. It exists to make scope checks explicit in APIs and business logic so callers can handle user-scoped and project-scoped memories differently without runtime type assertions.

## Example
```typescript
function handleScope(scope: MemoryScope) {
  switch (scope.kind) {
    case 'user':
      // handle user-scoped memory
      console.log('user scope');
      break;
    case 'project':
      // handle project-scoped memory
      // Note: the source for the 'project' variant appears truncated in the repository
      // — verify the full shape (e.g. project id) before accessing additional properties.
      console.log('project scope');
      break;
    default:
      // If you enable --strictNullChecks/--noImplicitReturns, this helps ensure exhaustiveness
      const _exhaustiveCheck: never = scope;
      return _exhaustiveCheck;
  }
}
```

## Notes
- The source declaration appears truncated for the 'project' variant; confirm any payload fields (such as a projectId) before relying on them.
- Because this is a plain discriminated union of objects, it is safe to perform narrowing via switch, if/else on kind, or type guards.

---

## MemoryType

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type

A string-literal union that enumerates the allowed categories for a memory record used by the webapp API. Use this type when accepting, validating, or annotating the category of a memory so the compiler enforces one of the allowed values ('user', 'feedback', 'project', 'reference').

## Remarks
This type is a compile-time constraint that documents and restricts the set of valid memory categories across the codebase. It is intended to be used in API request/response shapes, function parameters, and internal DTOs so callers and implementers share a single canonical set of category values.

## Example
```typescript
function createMemory(type: MemoryType, content: string) {
  // type is guaranteed to be one of the four literals at compile time
  return { id: generateId(), type, content };
}

// usage
const m = createMemory('project', 'Notes about the new feature');
```

## Notes
- This is a compile-time-only TypeScript construct; there is no runtime enforcement. Validate any external input (e.g., from HTTP requests) before trusting it as a MemoryType.
- When adding or removing variants, update any serialization, API docs, database schemas, and backend code that expects these exact string values to avoid mismatches.
- Prefer this narrow union over a plain string to get exhaustiveness checking in switches and better IDE completion.

---

## deleteMemory

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Deletes a memory resource on the server by ID. Use this when you need to remove a stored memory from the backend; the function performs an HTTP DELETE to /api/memories/:id, resolves when the operation completes, and throws on error responses (404 is treated as success and will not cause an exception).

## Remarks
This is a thin client helper that encapsulates the DELETE request for a single memory resource. The call is executed through the project’s withRefresh wrapper (used by other API helpers), and sends credentials ('include'), so it participates in cookie-based authentication flows.

## Example
```typescript
// Delete a memory and handle cancellation
const controller = new AbortController();
try {
  await deleteMemory('my-memory-id', controller.signal);
  console.log('Memory deleted');
} catch (err) {
  console.error('Failed to delete memory:', err);
}

// To cancel the request:
// controller.abort();
```

## Notes
- A 404 (not found) response is treated as non-fatal; the function will not throw for 404.
- Any non-OK response other than 404 causes an Error to be thrown with the HTTP status and statusText.
- The id parameter is URL-encoded via encodeURIComponent before being included in the path.
- The optional AbortSignal may be provided to cancel the underlying fetch request; if aborted, fetch will reject and the rejection will propagate to the caller.

---

## listMemories

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Fetches the list of memories for a given MemoryScope from the server and returns them as an array of MemoryDto. Use this helper when the UI or other client code needs to load memories for a particular scope; the function performs the HTTP request, handles the response status, and parses the JSON payload.

## Remarks
This function constructs the request URL via urlFor(scope) and performs the network call through the withRefresh wrapper, allowing any centralized request wrapper behavior (for example, token refresh or retries) to apply. It passes the provided AbortSignal to fetch so callers can cancel the request, and it includes credentials (cookies) with the request.

## Example
```typescript
const controller = new AbortController();
try {
  // `scope` should be a valid MemoryScope value in your application
  const memories = await listMemories(scope, controller.signal);
  console.log('Got memories:', memories);
} catch (err) {
  console.error('Failed to load memories', err);
}
// To cancel:
// controller.abort();
```

## Notes
- If the HTTP response has a non-OK status, the function throws an Error containing the status code and status text.
- The JSON response is cast to MemoryDto[]; a mismatch between the server payload and MemoryDto shape will surface at runtime.
- The fetch call uses credentials: 'include', so cookies are sent; ensure the server/CORS configuration allows credentialed requests.
- Aborting via the provided AbortSignal will cause fetch to reject (typically with an AbortError); callers should handle that if cancellation is a supported flow.

---

## saveMemory

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

```typescript
export async function saveMemory(
  request: SaveMemoryRequest,
  signal?: AbortSignal,
): Promise<MemoryDto>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `request` | [`SaveMemoryRequest`](../../../api/Gabriel.API/Contracts/Memories/MemoryDto.cs.md) | — |
| `signal` | `AbortSignal` | — |

**Returns:** ``Promise<MemoryDto>``


Sends a POST request to /api/memories with the provided SaveMemoryRequest serialized as JSON and returns the created MemoryDto. Use this helper when you want to create/save a memory from the web client and receive the server's canonical memory representation; it includes credentials (cookies) and supports cancellation via an optional AbortSignal.

## Remarks
This function uses a local helper (withRefresh) to perform the fetch — the wrapper is used consistently by API helpers to centralize concerns such as retries or authentication refresh before the actual request is executed. On success the response body is parsed as JSON and returned as MemoryDto.

## Example
```typescript
const req: SaveMemoryRequest = { title: 'Trip', content: 'Visited the lake' };
const controller = new AbortController();

try {
  const memory = await saveMemory(req, controller.signal);
  console.log('Saved memory', memory.id);
} catch (err) {
  console.error('Failed to save memory', err);
}
```

## Notes
- The request includes credentials: cookies will be sent with the request (credentials: 'include').
- If the response has a non-OK status, the function throws an Error containing the HTTP status and any response text.
- The function expects the server to return JSON that matches MemoryDto; malformed or non-JSON responses will cause parsing to fail.
- Pass an AbortSignal to cancel the request; aborted fetches will reject (propagated to the caller).

---

## urlFor

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Returns the HTTP path for the memories API, including an appropriate query string derived from the provided MemoryScope. Use this helper when constructing requests to /api/memories so callers don't need to build the query string manually.

## Remarks
This function maps a MemoryScope to query parameters: when scope.kind is 'all' it sets scope=all and will include projectId only if present; when scope.kind is 'project' it sets only projectId. If there are no parameters the plain path /api/memories is returned.

## Example
```typescript
// No query params
urlFor({ kind: 'all' }); // -> '/api/memories'

// All scope with project filter
urlFor({ kind: 'all', projectId: 'proj-123' }); // -> '/api/memories?scope=all&projectId=proj-123'

// Project-specific scope (projectId should be provided)
urlFor({ kind: 'project', projectId: 'proj-123' }); // -> '/api/memories?projectId=proj-123'
```

## Notes
- Ensure a valid projectId is provided when using kind === 'project'; omitting it may produce a query like projectId=undefined depending on runtime values.
- URLSearchParams percent-encodes values, so special characters in projectId will be escaped automatically.
- The function only sets scope=all for the 'all' kind; other kinds do not include a scope parameter.

---

## withRefresh

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Wraps an asynchronous request thunk, detects a 401 (unauthorized) response, attempts a session refresh once, and retries the request. Use this when you need centralized handling of authentication expiry for individual API calls: pass a function that executes the request so the wrapper can re-run it after refreshing the session.

## Remarks
This helper centralizes the common pattern of "try request → if 401 then refresh session → retry once → if still 401 treat the session as expired." It relies on external functions refreshSession() (which should return a truthy value on success) and signalSessionExpired() to notify the application when refresh fails. Accepting a thunk (doFetch) avoids coupling to a specific HTTP client and lets callers provide a fresh request invocation for the retry.

## Example
```typescript
// Common usage with the Fetch API
const response = await withRefresh(() => fetch('/api/memories', { method: 'GET', credentials: 'include' }));
if (!response.ok) throw new Error(`Request failed: ${response.status}`);
const data = await response.json();
```

## Notes
- doFetch may be invoked twice; ensure the request is safe to repeat (avoid non‑idempotent side effects on the first call).
- If refreshSession() returns a falsy value, the wrapper will not retry and will treat the session as expired (signalSessionExpired is called and an Error is thrown).
- Errors thrown by doFetch or refreshSession propagate out of withRefresh; this function only handles the 401/refresh/retry flow.
- Concurrent callers may each call refreshSession; if your refresh logic must be single-flight, coordinate that inside refreshSession or at a higher level.

---
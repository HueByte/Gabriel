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

Represents the serialized shape of a Memory resource exchanged with the API — that is, the DTO used when listing, retrieving, creating or updating memories. Use this type whenever you need the complete, transport-friendly representation of a memory record in client ↔ server communication.

## Remarks
MemoryDto encodes both identity and metadata for a memory: id and projectId indicate ownership/scope, type refers to the MemoryType enum, and name/description/body hold the content. createdAt and updatedAt are ISO 8601 timestamp strings provided by the service and should be parsed by clients when a Date object or locale-aware formatting is required. A null projectId denotes a user-scoped memory rather than a project-scoped one.

## Example
```typescript
import { MemoryDto, MemoryType } from './api/memories';

const example: MemoryDto = {
  id: "mem_01a2b3c4",
  projectId: null, // user-scope
  type: MemoryType.Text,
  name: "Onboarding notes",
  description: "Key points from onboarding session",
  body: "Remember to update the README and run the setup script.",
  createdAt: "2025-02-14T10:30:00Z",
  updatedAt: "2025-02-14T10:30:00Z"
};
```

## Notes
- projectId is nullable: null means the memory is in user-scope; a string means it belongs to a specific project.
- createdAt and updatedAt are plain ISO 8601 strings — parse them to Date objects on the client if you need date arithmetic or localized formatting.
- type is an enum (MemoryType); use the enum values rather than hard-coded strings to avoid mismatches.

---

## SaveMemoryRequest

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

```typescript
export interface SaveMemoryRequest
```


A DTO describing the payload sent to the backend when creating or updating a memory record. Use this shape whenever you call the API that saves a memory so the request contains the memory's scope (project or global), type, user-friendly name, optional description, and the memory content.

## Remarks
This interface represents the client-side contract for the "save memory" API. projectId is nullable to allow saving either a project-scoped memory (provide a projectId string) or a global/shared memory (pass null). The type field uses the MemoryType enum/union to categorize the memory; consumers should use one of that type's values rather than an arbitrary string.

## Example
```typescript
const requestForProject: SaveMemoryRequest = {
  projectId: 'proj_12345', // or null for a global memory
  type: MemoryType.SomeCategory, // replace with an actual MemoryType member
  name: 'User preference: dark mode',
  description: 'Stores whether the user prefers dark mode',
  body: '{ "prefersDark": true }'
};

const requestGlobal: SaveMemoryRequest = {
  projectId: null, // global memory
  type: MemoryType.SomeCategory,
  name: 'Global FAQ snippet',
  description: 'Short FAQ used across projects',
  body: 'Here is the FAQ content...'
};
```

## Notes
- projectId is explicitly nullable: pass a string for project-scoped memories or null to indicate a global/shared memory.
- All string fields (name, description, body) are required by the type system; if a value is not applicable, supply an empty string rather than omitting the property.
- Ensure the type value comes from the MemoryType definition to avoid validation errors on the server.

---

## MemoryScope

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type

A discriminated union that indicates the domain for a memory — either scoped to the current user or to a project. Reach for this type when code must choose where to read or write memories (for example, to decide between per-user storage and per-project storage) and when you want the intent of that decision encoded in the type system.

## Remarks
The union is discriminated by the string literal property `kind`; callers should narrow on `scope.kind` to access variant-specific data. This separation prevents accidental mixing of user-scoped and project-scoped memories and makes it explicit at API boundaries which domain a memory operation targets.

## Example
```typescript
function handleMemory(scope: MemoryScope) {
  if (scope.kind === 'user') {
    // operate on user-scoped memory
  } else {
    // scope.kind === 'project'
    // operate on project-scoped memory — use the project-specific fields
    // (verify the project variant's properties in the actual source)
  }
}
```

## Notes
- The source snippet available here is truncated: the `project` variant's properties are not present in the provided code. Verify the complete type definition before accessing any project-specific fields.
- Prefer narrowing (e.g., `if (scope.kind === 'user')`) or exhaustive switches rather than type assertions to avoid runtime errors if the shape changes.


---

## MemoryType

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type

A narrow string-union type that enumerates the allowed categories for stored "memories" in the web application API. Use this type when accepting, producing, or validating the category field of a memory object so callers get compile-time checks against the allowed literal values.

## Remarks
This type captures the canonical set of memory categories used across the webapp and backend ("user", "feedback", "project", "reference") so code that constructs or consumes memory payloads can rely on a single source of truth at compile time. It intentionally uses a string literal union instead of a numeric enum to preserve the exact textual values sent over the wire.

## Example
```typescript
// annotate a field or parameter
function createMemory(type: MemoryType, content: string) {
  return { type, content };
}

const m: MemoryType = 'project';
createMemory(m, 'Notes about the project timeline');

// narrowing example
function isReference(t: MemoryType): boolean {
  return t === 'reference';
}
```

## Notes
- The values are exact, case-sensitive string literals; mismatched casing will not type-check and will fail if sent to an API that expects these exact strings.
- This is a compile-time/type-system construct — there is no runtime enum object or reverse mapping. If you need a runtime list of valid values, build one from the union (e.g., a const array).
- When the backend introduces new memory categories, this type must be updated to keep front-end and API enums in sync.

---

## deleteMemory

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Deletes a memory resource on the server identified by the given id. Use this when you need to remove a memory via the web API; the function returns a Promise that resolves when the request completes successfully (or the resource is not found) and rejects for other HTTP failures.

## Remarks
This is a thin helper that performs a DELETE request to /api/memories/:id using fetch. The id is url-encoded with encodeURIComponent before being appended to the path. The actual network call is executed inside withRefresh; callers can provide an AbortSignal to cancel the request. The function does not parse or return any response body — success is indicated by the returned Promise resolving.

## Example
```typescript
const controller = new AbortController();
try {
  await deleteMemory('my-memory-id', controller.signal);
  console.log('Memory deleted (or did not exist).');
} catch (err) {
  console.error('Failed to delete memory:', err);
}
// To cancel:
// controller.abort();
```

## Notes
- A 404 response is treated as non-fatal: the function does not throw for status 404.
- For any other non-ok HTTP status the function throws a generic Error containing the status and statusText.
- The request includes credentials (cookies) via credentials: 'include'; ensure calling context expects this behavior.

---

## listMemories

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Fetches the list of memories for the given MemoryScope and returns them as a parsed array of MemoryDto objects. Use this helper when you need to load memories from the server with the current authentication context; it includes credentials and delegates token refresh handling to the shared withRefresh wrapper.

## Remarks
This function delegates two responsibilities to collaborators: urlFor(scope) builds the endpoint URL for the requested scope, and withRefresh(...) wraps the fetch call to handle token refresh or other retry logic. listMemories itself ensures credentials are sent (credentials: 'include'), supports cancellation via an AbortSignal, and throws a descriptive Error for non-OK HTTP responses.

## Example
```typescript
const controller = new AbortController();
try {
  const memories = await listMemories('user', controller.signal);
  console.log('Loaded memories:', memories);
} catch (err) {
  if (err.name === 'AbortError') {
    console.log('Fetch aborted');
  } else {
    console.error('Failed to load memories:', err);
  }
}
```

## Notes
- The function throws an Error when the HTTP response has a non-OK status; the error message includes the status code and status text.
- The result is cast to MemoryDto[] after parsing JSON; if the server returns unexpected JSON shape this may still succeed at runtime but consumers should validate shape if necessary.
- Passing an AbortSignal lets callers cancel the fetch; network errors and aborts will cause the returned Promise to reject.

---

## saveMemory

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Sends the provided SaveMemoryRequest to the server to persist a memory resource and returns the server's MemoryDto response. Use this when the client needs to create or save a memory on the backend; the function wraps a POST /api/memories fetch call, includes credentials, and accepts an optional AbortSignal to cancel the request.

## Remarks
This small API helper centralizes the HTTP details for creating/saving memories: it sets JSON headers, includes credentials (cookies), and delegates the actual network call to a withRefresh wrapper so callers benefit from any centralized behavior that wrapper provides (for example token refresh or retries). On non-OK responses it reads the response body (when possible) and throws a descriptive Error containing the HTTP status and server message.

## Example
```typescript
const controller = new AbortController();

try {
  const request: SaveMemoryRequest = {
    // populate required fields for your API
    title: 'Trip to Kyoto',
    notes: 'Cherry blossoms were in full bloom',
  };

  const saved = await saveMemory(request, controller.signal);
  console.log('Saved memory id=', saved.id);
} catch (err) {
  // handle network errors, server errors, or abort
  console.error('Failed to save memory', err);
}
```

## Notes
- If the response is not ok the function throws an Error whose message includes the HTTP status and the response body (the body read is best-effort; if reading fails an empty string is used).
- If the server returns invalid JSON, response.json() may throw; callers should handle that possibility.
- Passing an AbortSignal will cause fetch to abort and the promise to reject (typically with a DOMException); the abort is propagated to callers.

---

## urlFor

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Builds the REST endpoint URL for fetching memories according to the provided MemoryScope. Use this helper when constructing requests to /api/memories so callers don't need to assemble query strings or worry about encoding rules.

## Remarks
This small utility centralizes query-string construction for the memories API. It encodes the two supported scope shapes (an "all" scope which may optionally include a projectId, and a "project" scope which supplies only a projectId) and ensures parameters are URL-encoded via URLSearchParams. Keeping this logic here avoids duplication and inconsistent query formats across callers.

## Example
```typescript
// scope = all (no project)
urlFor({ kind: 'all' });
// -> '/api/memories?scope=all'

// scope = all with project filter
urlFor({ kind: 'all', projectId: 'proj-123' });
// -> '/api/memories?scope=all&projectId=proj-123'

// scope = project (projectId required)
urlFor({ kind: 'project', projectId: 'proj-456' });
// -> '/api/memories?projectId=proj-456'

// no query params produced -> base path
// (this happens only if MemoryScope yields no parameters)
// urlFor(/* such a scope */) -> '/api/memories'
```

## Notes
- Ensure a valid projectId is provided for kind === 'project'; passing undefined or null will result in the string "undefined"/"null" being placed in the query.
- For kind === 'all' the function always adds scope=all; projectId is included only when truthy.
- URLSearchParams handles percent-encoding for you (e.g., spaces become + or %20 depending on environment), so callers do not need to encode values themselves.

---

## withRefresh

> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

Wraps a fetch-style operation and automatically attempts a session refresh if the initial response is HTTP 401. Call this when you want a single transparent retry after an authentication expiry and a clear failure (and notification) when the session cannot be refreshed.

## Remarks
withRefresh centralizes a common authentication pattern: perform a request, and if it fails because the session is unauthorized (401), try to refresh the session once and retry the request. If the retry still results in 401 the function signals that the session has expired and throws an Error so callers can handle sign-in flows or show a message.

## Example
```typescript
// Typical usage wrapping a fetch call
async function getMemories(): Promise<Response> {
  return withRefresh(() => fetch('/api/memories', { method: 'GET', credentials: 'include' }));
}

// Caller handles both successful responses and the thrown session-expired error
try {
  const resp = await getMemories();
  if (!resp.ok) {
    // handle other non-OK statuses
  }
  const data = await resp.json();
  // use data
} catch (err) {
  // err will be an Error with message 'Session expired. Please sign in again.' if refresh failed
}
```

## Notes
- doFetch may be invoked twice: ensure the operation is idempotent or that its request body/stream can be safely re-sent.
- Only HTTP 401 triggers the refresh-and-retry flow; other status codes are returned unchanged.
- If refreshSession returns a falsy value (refresh failed), withRefresh will treat the session as expired, call signalSessionExpired(), and throw.
- Callers must catch the thrown Error to handle the "session expired" case; the function does not perform UI navigation itself.

---
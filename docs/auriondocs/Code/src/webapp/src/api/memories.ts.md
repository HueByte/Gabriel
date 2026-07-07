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

```typescript
export interface MemoryDto
```


MemoryDto is a lightweight data transfer object that describes a memory item in the system. It carries the identifier, optional project scope (projectId), a memory category (MemoryType), a human-friendly name and description, the full content body, and timestamps for creation and last update. Consumers use MemoryDto when exchanging memory data across API boundaries or between application layers—for example when listing memories, creating a new memory, or updating an existing one. The projectId being null denotes a user-scoped memory, while a non-null value ties the memory to a particular project context.

## Remarks
MemoryDto serves as the API-facing contract for memory resources. It decouples transport concerns from the internal domain model and provides a stable shape for serialization and deserialization across the client/server boundary. It is intentionally free from business logic, validation, or mutating behavior.

## Notes
- createdAt and updatedAt are ISO 8601 strings; consumers should parse them with standard Date parsing and consider time zones.
- The body field contains the memory's content and may be lengthy; consider API limits and encoding constraints when transmitting large bodies.
- The projectId field is nullable; null indicates user-scoped memory while a non-null value indicates project-scoped memory, which can influence authorization and filtering in API calls.

---

## SaveMemoryRequest
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

```typescript
export interface SaveMemoryRequest
```


SaveMemoryRequest is a data transfer object that represents the payload used when persisting a memory entry through the application's API. It carries the minimal information required to create or update a memory: an optional project association, a memory type category, a human-friendly name, a description, and the body content of the memory.

## Remarks
SaveMemoryRequest exists to transport memory data between the UI layer and backend without embedding behavior. The MemoryType categorizes the memory for filtering and organization, while projectId scopes the memory to a specific project or remains null for unassociated memories. As a plain DTO, server-side validation and persistence rules live outside this interface.

## Notes
- Null projectId means the memory isn't tied to a project and should be treated accordingly by the backend.
- MemoryType must be a valid API category; mismatches will cause validation errors.
- This is a pure data carrier; it should not include business logic or side effects.

---

## MemoryScope
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type alias

```typescript
export type MemoryScope =
  | { kind: 'user' }
  | { kind: 'project'; projectId: string }
  | { kind: 'all'; projectId?: string };
```


MemoryScope is a discriminated union that encodes the scope for memory-related operations in the API. Use it to express whether memory operations should apply to the current user, a specific project, or all memories (with an optional project filter).

## Remarks
This type centralizes the memory-scoping decision, making API calls more expressive and type-safe. It supports exhaustive handling via switch statements and clarifies intent across collaborators. The optional projectId on the 'all' variant lets you narrow the broad view without introducing a separate flag.

## Example
```typescript
const userScope: MemoryScope = { kind: 'user' };
const projectScope: MemoryScope = { kind: 'project', projectId: 'proj-123' };
const allScope: MemoryScope = { kind: 'all' };
const allWithProject: MemoryScope = { kind: 'all', projectId: 'proj-123' };
```

## Notes
- The 'all' variant's projectId is optional; be aware of how your backend interprets this field when combining across projects.
- When consuming MemoryScope, ensure all kinds are handled to keep behavior correct and future-proof.

---

## MemoryType
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type alias

```typescript
export type MemoryType = 'user' | 'feedback' | 'project' | 'reference'
```


MemoryType is a TypeScript alias that enumerates the allowed string values used to categorize memory records in the application. It restricts values to 'user', 'feedback', 'project', or 'reference', enabling type-safe labeling and consistent downstream processing such as filtering, analytics, or UI decisions.

## Remarks
This abstraction centralizes memory category semantics, reducing scattered string literals and making it easy to evolve categories in a single place. It cooperates with memory-related APIs, components, and services by providing a single, shared source of truth for the memory category, which improves code reuse and reduces bugs from mismatched literals.

## Example
```typescript
function describeMemory(mem: MemoryType): string {
  switch (mem) {
    case 'user':
      return 'A memory created by a user';
    case 'feedback':
      return 'Feedback about memory';
    case 'project':
      return 'Memory related to a project';
    case 'reference':
      return 'Reference memory used for lookup';
  }
}
```

## Notes
- The allowed values are exact string literals; using any other string will fail at compile time when assigned to MemoryType.
- If you receive memory type values from external data (e.g., API payloads), validate or cast to MemoryType to preserve type safety.
- MemoryType is a type-level construct; at runtime it collapses to a plain string, so performance or runtime behavior is unaffected by its existence.

---

## deleteMemory
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

```typescript
export async function deleteMemory(id: string, signal?: AbortSignal): Promise<void>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `id` | `string` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<void>`


Deletes a memory by ID from the server using an HTTP DELETE request to /api/memories/{id}. The ID is URL-encoded, cookies are included, and the request is wrapped in withRefresh to handle common ancillary concerns such as authentication token refresh. On success, the function completes and resolves to void. If the memory does not exist (404), the call is treated as a no-op; for any other non-success status, an Error is thrown carrying the HTTP status and status text.

## Remarks
The withRefresh wrapper suggests a cross-cutting resilience concern (e.g., token refresh or retry) is shared across API calls. This function encapsulates the delete semantics and the server's expected error handling (treating 404 as non-fatal) so callers can rely on consistent behavior without re-implementing HTTP boilerplate.

## Example
```typescript
await deleteMemory("abc123");
```

## Notes
- 404 responses are treated as a no-op; the caller cannot distinguish between 'not found' and 'already removed' from the return value.
- The request includes credentials; ensure server permits credentials for CORS.
- An AbortSignal can cancel the request; callers should handle cancellation when needed.

---

## listMemories
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

```typescript
export async function listMemories(scope: MemoryScope, signal?: AbortSignal): Promise<MemoryDto[]>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `scope` | `MemoryScope` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<MemoryDto[]>`


Fetches the memories for a given MemoryScope by performing an authenticated HTTP GET to the endpoint derived from the scope, while supporting cancellation via an AbortSignal. It returns MemoryDto[] on success and throws an Error if the server responds with a non-OK status.

## Remarks
ListMemories functions as a focused data-access facade over the Memories API. It uses withRefresh to ensure authentication tokens are refreshed when needed before issuing the request and it centralizes error handling for memory retrieval, keeping callers free from repetitive fetch boilerplate.

## Notes
- Aborted requests reject the promise (via the AbortSignal) with an AbortError.
- Non-OK responses lead to a thrown Error with a message containing the HTTP status.

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
| `request` | `SaveMemoryRequest` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<MemoryDto>`


Sends the provided SaveMemoryRequest to the server as a JSON payload via a POST to /api/memories, including credentials (cookies) and using the SaveMemoryRequest structure defined on the client-server contract. The call is wrapped in withRefresh to ensure the user’s session is refreshed if needed, and it accepts an optional AbortSignal to cancel the request; on a successful response it returns a MemoryDto, while non-OK responses are turned into a descriptive Error that includes the HTTP status and server message.

## Remarks
Acts as a dedicated mutation helper for persisting memories from the client UI. It centralizes HTTP concerns (content-type, credentials, error handling, and response parsing) so callers don’t duplicate boilerplate and can rely on a stable MemoryDto return type. By delegating authorization refresh to withRefresh, it reduces the likelihood of token-expiry errors interrupting memory creation.

## Notes
- If the server returns an error body, the code attempts to read it as text; if reading the body fails, the error message falls back to the HTTP status alone.

---

## urlFor
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

```typescript
function urlFor(scope: MemoryScope): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `scope` | `MemoryScope` | — |

**Returns:** `string`


Constructs the API URL used to fetch memories for a given MemoryScope. It accepts a MemoryScope with kind 'all' or 'project' and builds a relative endpoint by appending appropriate query parameters via URLSearchParams. When there are no parameters, it returns /api/memories without a query string; otherwise it returns /api/memories?{params}. Callers use this helper to keep memory-fetching logic consistent and to avoid duplicating URL construction across the codebase.

## Remarks
Encapsulates the rules for encoding scope-based filters into the memories API endpoint, so changes to filtering semantics are localized. It relies on URLSearchParams to handle proper encoding and to drop the query string when no filters exist, producing a clean base URL. This function is a tiny utility that cooperates with fetch-like calls to fetch memories.

## Notes
- If scope.kind === 'project' and scope.projectId is missing, the query will include projectId=undefined, which is probably unintended; provide a valid projectId when using the 'project' kind.
- The function returns a relative path (starting with /api/memories) suitable for concatenation with a base API origin.
- This symbol does not perform network requests; it only formats the URL.

---

## withRefresh
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** function

```typescript
async function withRefresh(doFetch: () => Promise<Response>): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `doFetch` | `() => Promise<Response>` | — |

**Returns:** `Promise<Response>`


withRefresh executes a fetch-like callback and, if the response status is 401, attempts to refresh the session and retry once. If the second attempt still results in 401, it signals that the session has expired and throws a dedicated error.

## Remarks
Centralizes session-renewal logic for API calls driven by the doFetch callback pattern, reducing boilerplate in callers and keeping authentication concerns in one place. It relies on refreshSession to obtain a new token and on signalSessionExpired to trigger the proper user-facing flow when renewal fails. By isolating this behavior, the function makes its usage intent explicit: you wrap a request to automatically handle common auth edge cases without duplicating retry logic across many call sites.

## Example
```typescript
// Example
const response = await withRefresh(() => fetch('/protected/resource'));
```

## Notes
- Only retries once after a 401. If the second attempt also fails with 401, a session-expired error is thrown.
- Errors from doFetch (e.g., network failures) propagate to the caller; this function does not catch them.

---
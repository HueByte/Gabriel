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


MemoryDto defines the API-facing shape of a memory record. It carries the memory’s id, scope (projectId; null indicates a user-scoped memory), type classification (MemoryType), and descriptive content (name, description, body), along with audit timestamps (createdAt, updatedAt). It is used when transferring memory data between client and server—such as listing, creating, or updating memories—so callers receive a stable, explicit structure and can rely on a consistent mapping across layers. The MemoryType field enables consistent filtering and categorization of memories by kind.

## Remarks
MemoryDto exists to decouple API contracts from storage details; it defines the contract the UI and API share. The projectId field encodes scope, enabling both user-scoped and project-scoped memories to be represented by the same interface. CreatedAt/UpdatedAt support synchronization and ordering; they should be treated as authoritative for freshness rather than derived values.

## Notes
- projectId can be null to indicate user-scoped memories; callers should handle both scopes.
- createdAt and updatedAt are ISO 8601 strings; parse them to Date objects with explicit timezone handling on the client.
- The body field may contain lengthy content; rendering should sanitize content and consider lazy-loading or truncation in listings to avoid performance issues when displaying many memories.


---

## SaveMemoryRequest
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

```typescript
export interface SaveMemoryRequest
```


SaveMemoryRequest defines the shape of the payload sent to the memories API when persisting a new memory item. Use it when constructing a client-side request to create a memory entry; it bundles optional project context, the memory type, a display name, a short description, and the actual memory content in body, rather than assembling these fields ad-hoc.

## Remarks
SaveMemoryRequest serves as a typed contract between the client and server for saving memory snippets. It centralizes the payload structure, enabling validation and consistent API usage across the codebase. The nullable projectId supports saving memories that are not tied to a specific project; when null, the server should apply a global or default scope.

## Notes
- projectId may be null to indicate a global or unscoped memory.
- body can be large; ensure server limits and client-side encoding are respected.
- MemoryType must be a valid MemoryType value known to the API.

---

## MemoryScope
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type alias

```typescript
export type MemoryScope =
  |
```


MemoryScope defines the memory context used by the memory subsystem. It is a discriminated union with two variants: user and project. Each variant is an object with a kind property; the user variant marks memory that is scoped to an individual user, while the project variant marks memory scoped to a particular project. Consumers use MemoryScope to route storage or caching operations to the appropriate backing store without duplicating logic across scopes.

## Remarks
By encoding scope as a single value, MemoryScope enables centralized routing of memory-related operations and helps keep concerns separated: the caller doesn't need to know where memory is stored, just which scope to use. It also makes exhaustive handling possible with discriminated unions, catching missing scope variants at compile time.

## Example
```typescript
function getMemoryStore(scope: MemoryScope) {
  switch (scope.kind) {
    case 'user':
      return getUserMemoryStore();
    case 'project':
      return getProjectMemoryStore();
  }
}
```

## Notes
- Ensure exhaustive handling of the kind discriminator; add a default case only if you truly intend to support a runtime variant.
- If the project variant carries additional payload (e.g., identifiers), narrow the type before accessing those fields.
- Keep consumers aligned with any storage backends that implement per-user or per-project caches.

---

## MemoryType
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type alias

```typescript
export type MemoryType = 'user' | 'feedback' | 'project' | 'reference'
```


MemoryType is a TypeScript type alias that defines the allowed categories for memory records within the memories API. It restricts memory types to one of four string literals: 'user', 'feedback', 'project', or 'reference'. This type is used wherever a memory item must be classified, ensuring that only valid categories are assigned and enabling type-safe handling of memories by consumers of the API.

## Remarks
This type centralizes the valid memory categories, enabling consistent handling of memories across features like filtering, display, and persistence. It prevents invalid category values from being used and makes it easy to extend with additional kinds in the future by updating a single definition.

## Notes
- If memory data comes from external input (e.g., API responses or user input), perform runtime validation to guard against values outside the defined union, since TypeScript types are erased at runtime.

```typescript
const isMemoryType = (v: unknown): v is MemoryType =>
  v === 'user' || v === 'feedback' || v === 'project' || v === 'reference';
```

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


Deletes a memory by id by issuing an HTTP DELETE to /api/memories/{id}, URL-encoded. It uses withRefresh to ensure the request benefits from any automatic authentication refresh, and it includes credentials in the request. An optional AbortSignal can cancel the operation. The function resolves when the response is OK or when the resource is not found (404); for any other non-success status, it throws an Error with the status code and text.

## Remarks
deleteMemory centralizes the deletion semantics for memory resources. By treating 404 as not an error, callers can safely issue deletes without first verifying existence, reducing boilerplate and extra round-trips. The wrapper around fetch encapsulates session maintenance (such as token refresh), so callers don't need to manage authentication details for this operation.

## Notes
- Aborting the request via the provided AbortSignal will reject the Promise with an AbortError; callers should handle cancellation if needed.
- A 404 response is treated as a non-error; no exception is thrown in that case, and the memory is considered non-existent.
- The ID is URL-encoded to support special characters in IDs; ensure IDs with reserved URL characters are handled correctly.

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


Fetches the list of memories for the specified MemoryScope by performing an authenticated GET request to the API endpoint derived from the scope. It uses withRefresh to handle token refresh flows and supports cancellation via an optional AbortSignal. If the response indicates failure, it throws a descriptive error; on success, it returns the response parsed as an array of MemoryDto.

## Remarks
Centralizes memory retrieval behind a single API surface, shielding callers from URL construction (urlFor(scope)) and HTTP error handling. It also ensures the consumer always receives MemoryDto objects and lets withRefresh manage authentication concerns, reducing duplication across UI code.

## Example
```typescript
// Example usage
const scope = MemoryScope.All; // or any valid MemoryScope value
const memories = await listMemories(scope, abortController?.signal);
```

## Notes
- Non-OK HTTP responses throw an Error with a message that includes the status code and status text; callers should catch and handle accordingly.
- Abort signals allow cancellation; pass an AbortController.signal when you need to cancel in-flight requests.
- This function relies on withRefresh to refresh authentication; in contexts without that wrapper, you may need to implement your own refresh strategy.

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


saveMemory persists a memory by POSTing a SaveMemoryRequest to /api/memories and returns the resulting MemoryDto. The call runs through withRefresh to refresh authentication if needed, sends JSON payload with credentials included, and resolves to MemoryDto on success; on non-ok responses it throws an error containing the HTTP status and any server-provided text.

## Remarks
Encapsulates the memory-creation API; centralizes error handling and the authentication refresh flow. The wrapper ensures a valid authentication state before attempting the request, so callers don't need to manage token refresh themselves.

## Notes
- If the server returns non-JSON or payload not matching MemoryDto, the JSON parse or cast may fail at runtime.
- The error message includes the status code and optional server text; if the server omits text, the message will contain only the status.

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


Constructs the API path to fetch memories for a given scope. It uses URLSearchParams to assemble a query string and returns the base path '/api/memories' with an attached query when parameters exist, or the base path when no parameters are present. If scope.kind is 'all', the produced URL includes scope=all and, if provided, a projectId; if scope.kind is 'project', the URL includes only a projectId.

## Remarks
Centralizes how memory data is requested, shielding callers from the exact query parameter names and the base path. It guarantees a consistent URL shape across the web app and makes it straightforward to adapt to backend changes without touching call sites.

## Example
```ts
// All memories across projects
urlFor({ kind: 'all' });

// All memories for a specific project
urlFor({ kind: 'all', projectId: 'project-42' });

// Memories for a specific project only
urlFor({ kind: 'project', projectId: 'project-42' });
```

## Notes
- If kind === 'project' and projectId is missing or falsy, the function may return '/api/memories' with no filtering, which effectively fetches all memories.
- URLSearchParams handles encoding of projectId values; no manual encoding is required.
- The function returns a relative path; callers should resolve to an absolute URL if needed based on their fetch/HTTP client context.

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


Executes a fetch-like operation and transparently handles session renewal when authentication fails. It accepts a doFetch function that returns a `Promise<Response>`, runs it, and if the response has status 401, attempts to refresh the session via refreshSession(). If the refresh succeeds, it retries the original fetch once. If the retry still results in 401, it signals that the session has expired and throws an error prompting the user to sign in again. The function returns the final Response from either the initial or retried request.

---
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


MemoryDto is the serializable shape used to transfer memory data between the client and server (e.g., when listing, creating, or updating memories). Use it whenever you need to read or write a memory payload from API responses or requests; the projectId being null signals a user-scoped memory, while a non-null value ties it to a project.

## Remarks
MemoryDto serves as a boundary contract between the API layer and UI or business logic. It decouples transport data from domain models, allowing the server to evolve internal representations without breaking clients. The projectId field expresses memory scope, with null representing user-scoped memories and a string capturing project association. The ISO 8601 strings for createdAt/updatedAt ensure consistent temporal data across systems.

## Notes
- projectId can be null; callers must handle both user-scoped and project-scoped memories.
- createdAt/updatedAt are ISO 8601 strings; consider time zone and parsing across environments.
- MemoryDto is an interface; you cannot instantiate it directly—provide a concrete object that matches the shape.

---

## SaveMemoryRequest
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** interface

```typescript
export interface SaveMemoryRequest
```


Represents the payload required to save a memory entry in the system. It groups the memory’s metadata and content into a single object and enforces a consistent contract for persistence operations. The memory is associated with a project via projectId (which may be null to indicate no project), categorized by type using MemoryType, and described by a name, a brief description, and the body content.

## Remarks

DTO semantics: SaveMemoryRequest is a Data Transfer Object that decouples UI concerns from persistence details. It provides a stable boundary for API calls and helps centralize validation rules (e.g., required fields such as name and body) before reaching the backend. The MemoryType dependency signals how the memory should be treated, indexed, or surfaced by the UI and storage layer.

## Notes

- projectId is nullable; callers must handle the null case when saving a memory.
- The body field is a string; consider any backend length constraints and ensure input sanitation if the content will be rendered in the UI or stored in a text field.

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


MemoryScope defines the subset of memories an API should operate on. It’s a discriminated union over three scopes: user, project, and all. Use MemoryScope instead of ad-hoc objects to get type-safe branching and clear intent when calling memory-related APIs.

## Remarks
The "kind" field is the discriminant; each variant encodes its required data: 'user' has no extra data, 'project' requires a projectId, 'all' may include an optional projectId.

In consumer code, you typically switch on scope.kind and handle 'user', 'project', and 'all' distinctly; the TypeScript compiler helps ensure you cover all cases.

The 'all' variant's projectId?: string supports contexts where you want the global scope or optionally refine to a specific project in the same branch.

## Example
```typescript
const scopeUser: MemoryScope = { kind: 'user' };
const scopeProject: MemoryScope = { kind: 'project', projectId: 'proj-42' };
const scopeAllGlobal: MemoryScope = { kind: 'all' };
const scopeAllWithProject: MemoryScope = { kind: 'all', projectId: 'proj-99' };
```

## Notes
- When consuming MemoryScope in a switch, cover all cases to get exhaustive checks, e.g., using a final 'never' check.
- Be mindful that the 'all' variant allows an optional projectId; APIs that require a projectId should not rely on presence of that field in 'all' without checking.

---

## MemoryType
> **File:** `src/webapp/src/api/memories.ts`  
> **Kind:** type alias

```typescript
export type MemoryType = 'user' | 'feedback' | 'project' | 'reference'
```


MemoryType is a TypeScript type alias that enumerates the categories a memory entity can belong to in the application. It restricts values to the four string literals: 'user', 'feedback', 'project', and 'reference'. This provides a compile-time contract for memory data, enabling safe tagging, filtering, and rendering based on memory kind without incurring runtime overhead.

## Remarks
Using a literal union type keeps runtime overhead minimal while offering strong typing across memory-related APIs. It centralizes the domain concept of what a memory can be, allowing components to branch, format, and validate behavior consistently according to memory kind.

## Example
```typescript
function describeMemoryType(type: MemoryType): string {
  switch (type) {
    case 'user':
      return 'User-generated memory';
    case 'feedback':
      return 'Feedback about a memory';
    case 'project':
      return 'Project-associated memory';
    case 'reference':
      return 'Reference material';
  }
}
```

## Notes
- MemoryType is a type-level construct; at runtime it is simply a string. Validate external data before assigning to a MemoryType-typed field.
- If you extend MemoryType with new literals, remember to update all consuming logic (e.g., switches) to maintain exhaustiveness.

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


Deletes a memory resource by ID using an HTTP DELETE request. It wraps the fetch call with a helper (withRefresh) to accommodate authentication refresh scenarios and includes credentials for session-based access. The memory ID is URL-encoded to avoid path issues, and the operation supports cancellation via an AbortSignal. If the server responds with 404, the function treats this as a non-error (the memory may already be gone); for any other non-success status, it throws an Error that includes the status code and text. Use this helper when you need a straightforward, consistent deletion of a memory by its identifier from the server, rather than composing the fetch logic yourself.

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


Fetches a list of MemoryDto records for a given MemoryScope by issuing an authenticated HTTP request. It delegates the network call to withRefresh, which implies a refresh flow may run prior to the actual fetch. The request is performed with credentials: 'include' and supports cancellation via an optional AbortSignal. If the server responds with a non-OK status, the function throws an Error containing the HTTP status and status text; on success, it returns the response parsed as MemoryDto[].

## Remarks
This function centralizes the retrieval of memories scoped by MemoryScope, abstracting away the details of the underlying HTTP call and error handling from callers. The use of withRefresh suggests that token or session refresh logic is applied before the request, keeping callers focused on data retrieval rather than authentication lifecycle. The AbortSignal parameter enables cancellation, which is useful for component unmounts or user-initiated cancellations.

## Notes
- The returned value is a plain JSON interpretation cast to MemoryDto[]. If the server returns data that does not conform to MemoryDto, the cast may be unsafe at runtime. Consider adding runtime validation if strict guarantees are required.
- The request includes credentials, so cookies or other credentials are sent; ensure the server is configured for credentialed requests (CORS with credentials allowed).
- Aborting the request will reject the promise (likely with an AbortError); callers should handle cancellation accordingly.
- The exact behavior of withRefresh is implementation-defined; callers should not rely on specific token refresh timing beyond the fact that a refresh may occur before the fetch.

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


Saves a memory by sending a POST request to /api/memories with a JSON body derived from SaveMemoryRequest, returning the created MemoryDto. It is the canonical helper to persist memory data to the server instead of constructing fetch calls ad-hoc, and it handles JSON serialization, credentialed requests, and cancellation support via an optional AbortSignal.

The internal withRefresh wrapper ensures authentication is refreshed as needed before the request, providing a smoother experience for callers who would otherwise implement token refresh logic themselves. On non-OK HTTP responses, the function reads any response text and throws an Error that includes the status code and server message.

## Remarks
By encapsulating the HTTP interaction, this symbol enforces a stable contract for saving memories and reduces boilerplate at call sites. It also centralizes error semantics, surfacing a consistent Error message that combines the HTTP status with server-provided details when available. If the Memories API changes (path, payload shape, or success type), updating this single function keeps the rest of the UI code insulated from those changes.

## Notes
- The AbortSignal parameter can cancel the request; callers should handle cancellation errors as part of their control flow.
- The response is parsed as MemoryDto; if the server returns non-JSON or a mismatched shape, a runtime error may occur at parse time.
- The request uses credentials: 'include'; ensure the backend supports cookies or other credentials for this endpoint.

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


Constructs the canonical API endpoint URL for listing memories filtered by a given MemoryScope. It encodes the scope's kind and optional projectId into a query string and returns the full path to fetch memories from the backend. Use this helper instead of composing the URL manually to ensure consistent parameter names and encoding.

## Remarks
This helper centralizes how memory queries are requested from the client, ensuring uniform semantics across the UI. It relies on URLSearchParams to handle proper encoding and only adds scope-related parameters when they are present. If future scope options are added, this function is the single place to update the URL shape. It also ensures the base endpoint '/api/memories' is returned when no query parameters are necessary.

## Example
```ts
urlFor({ kind: 'all' })        // '/api/memories?scope=all'
urlFor({ kind: 'all', projectId: '123' }) // '/api/memories?scope=all&projectId=123'
urlFor({ kind: 'project', projectId: '123' }) // '/api/memories?projectId=123'
```

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


withRefresh wraps a fetch-like operation and automatically handles a one-time re-authentication flow. It executes the provided doFetch, and if it returns 401, it tries to refresh the session via refreshSession; on success it retries the fetch. If the retried response is still 401, it signals session expiration and throws an error prompting the user to sign in again. Use this when you want a single, centralized retry-on-unauthorized pattern around authenticated API calls.

## Remarks
This function centralizes the common "retry after refresh" pattern, preventing duplication across call sites. It ties together refresh, retry, and session expiration signaling; callers don't need to manage token refresh logic themselves. It assumes a boolean return from refreshSession indicates whether a refresh occurred and that signalSessionExpired will produce a user-visible session timeout flow.

## Example
```typescript
// Example: fetch a protected resource with effortless session refresh
const res = await withRefresh(() => fetch('/api/memories'));
```

## Notes
- If the initial doFetch fails with 401 and the refresh fails, the function will throw; you should handle errors around withRefresh to surface to the user or redirect.
- It only performs a single refresh attempt; multiple attempts or backoff are not implemented.

---
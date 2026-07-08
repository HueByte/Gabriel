# sequence.ts

> **Source:** `src/webapp/src/api/sequence.ts`

## Contents

- [GabrielSequence](#gabrielsequence)
- [GabrielSequenceMetadata](#gabrielsequencemetadata)
- [SequenceSource](#sequencesource)
- [doFetch](#dofetch)
- [fetchGabrielSequence](#fetchgabrielsequence)
- [urlFor](#urlfor)

---

## GabrielSequence
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

```typescript
export interface GabrielSequence
```


GabrielSequence describes the data structure for a Gabriel animation sequence used by the rendering pipeline; you would create or consume this object whenever you need to package or read the sequence's color palette, per-frame indices, version, and associated metadata in a single, strongly typed container.

## Remarks

By separating the color palette from the per-frame indices, this shape enables efficient storage and reuse of a single palette across all frames. Each frame is represented as a 256-length array of palette indices, allowing light-weight, index-based pixel data that can be mapped to colors via the palette. The metadata field (GabrielSequenceMetadata) carries additional descriptors about the sequence, decoupling presentation concerns from the raw color-data payload. The interface acts as a cohesive contract that ties together versioning, color data, per-frame references, and descriptive metadata for downstream rendering or processing.

## Notes

- The interface does not itself enforce runtime invariants (for example, that frames.length === 64 or that every frame has length 256); callers should validate shapes before processing.

---

## GabrielSequenceMetadata
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

```typescript
export interface GabrielSequenceMetadata
```


GabrielSequenceMetadata is a small data contract that describes the provenance of a Gabriel sequence. It exposes the numeric seed used to initialise generation, the exact time of generation as an ISO 8601 string, and an optional textual snapshot of the generator's state.

## Remarks
This interface separates concerns between sequence generation and its consumption by clients or APIs, allowing callers to display provenance or reproduce results by reusing the same seed. It also makes the generatedAt timestamp explicit and serialisable, avoiding runtime Date objects in payloads and ensuring consistent cross-system logging.

## Example
```typescript
const meta: GabrielSequenceMetadata = {
  seed: 12345,
  generatedAt: "2024-11-05T14:23:45Z",
  stateSummary: "initialized with seed 12345"
};
```

## Notes
- The seed is a numeric value used to initialise the sequence; using the same seed should yield the same sequence from the generator (assuming deterministic behaviour).
- generatedAt must be a valid ISO 8601 timestamp; using a UTC timestamp (ending with Z) avoids timezone misalignment.
- stateSummary is optional and may be null if no human-readable state is available.

---

## SequenceSource
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** type alias

```typescript
export type SequenceSource =
  | { kind: 'conversation'; conversationId: string }
  | { kind: 'project'; projectId: string };
```


SequenceSource is a discriminated union that encodes the origin of a sequence. It has two variants: a conversation variant carrying a conversationId, and a project variant carrying a projectId. This type lets API bodies or UI logic accept a single source value while preserving a strong separation of concerns through the kind discriminator.

## Remarks
This abstraction centralizes the concept of a sequence's origin, enabling safe branching and consistent handling across components that consume or produce sequence-related data. By switching on kind, you can reliably access the corresponding id (conversationId for kind: 'conversation', or projectId for kind: 'project'), which helps keep payloads aligned and reduces the chance of mixing identifiers from different contexts. If new origins are added in the future, the union can be extended without scattering origin-specific fields across call sites.

## Notes
- Always narrow by kind before reading the id: if source.kind === 'conversation', read source.conversationId; if source.kind === 'project', read source.projectId.
- Treat the sequence source as a single payload wrapper: its shape enforces that only the relevant id field is used for a given origin, aiding serialization and validation.

---

## doFetch
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

```typescript
function doFetch(source: SequenceSource, signal?: AbortSignal): Promise<Response>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `source` | `SequenceSource` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<Response>`


doFetch constructs a fetch call for the provided SequenceSource by deriving the endpoint with urlFor(source) and issuing a request with credentials included. It returns a `Promise<Response>` and supports cancellation via the optional AbortSignal.

## Remarks
This small wrapper centralizes the common HTTP pattern used to retrieve sequence data, so callers don't need to know how the URL is formed or the credentials policy. By encapsulating the fetch invocation, it becomes easier to swap in a different transport (e.g., a test double) or add shared behavior (like global error handling or response normalization) in one place. It also communicates intent clearly: this is the standard path for obtaining a sequence resource from the backend.

## Notes
- Using credentials: 'include' means cookies and HTTP authentication data are sent; ensure the server's CORS policy allows credentials.
- AbortSignal support allows callers to cancel in-flight requests; callers should handle DOMException when aborted.
- urlFor(source) must produce a valid URL; miswiring can cause runtime errors; validate inputs at call sites.

---

## fetchGabrielSequence
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

```typescript
export async function fetchGabrielSequence(
  source: SequenceSource,
  signal?: AbortSignal,
): Promise<GabrielSequence>
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `source` | `SequenceSource` | — |
| `signal` | `AbortSignal` | — |

**Returns:** `Promise<GabrielSequence>`


FetchGabrielSequence retrieves a GabrielSequence for the specified SequenceSource. If the initial request returns 401, it triggers a session refresh and retries once before failing with a session-expired error; for other HTTP failures it throws a descriptive error, and on success it returns the parsed GabrielSequence.

## Remarks
This function acts as a focused wrapper around the underlying fetch logic, centralizing authentication handling and error semantics for sequence retrieval. By encapsulating the refresh-and-retry pattern, it keeps callers free from boilerplate and ensures a consistent user experience when sessions expire. It coordinates with refreshSession and signalSessionExpired to maintain a single, predictable flow for expired credentials.

## Notes
- It retries only once after a 401; subsequent 401 triggers a SessionExpired signal and a thrown error.
- Non-OK responses (e.g., 500) produce a generic fetch failure error that includes the status code and status text.
- If the service returns invalid JSON for GabrielSequence, parsing will throw a runtime error.

---

## urlFor
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

```typescript
function urlFor(source: SequenceSource): string
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `source` | `SequenceSource` | — |

**Returns:** `string`


Constructs the correct API URL for a SequenceSource by inspecting its kind. It returns a conversation sequence URL when the source represents a conversation, otherwise it returns the project sequence URL.

## Remarks
Pure and stateless, urlFor centralizes endpoint URL construction to avoid duplicating string literals across the codebase. It relies on a discriminated union (SequenceSource) to choose the appropriate path and uses encodeURIComponent to safely encode IDs in the URL.

## Example
```typescript
// Example usage demonstrating both branches
const convoSrc: SequenceSource = { kind: 'conversation', conversationId: '123/abc' };
urlFor(convoSrc); // "/api/conversations/123%2Fabc/sequence"

const projSrc: SequenceSource = { kind: 'project', projectId: 'proj-42' };
urlFor(projSrc); // "/api/projects/proj-42/sequence"
```

## Notes
- If a new SequenceSource kind is added, this function will need to be updated to handle the new path.

---
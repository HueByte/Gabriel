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


Represents a sequence of frames using a palette-based color system, designed for efficient storage and rendering of graphical animations or images. Each sequence includes a version number, a palette defining RGB colors, an array of frames where each frame is an array of palette indices, and associated metadata describing the sequence.

## Remarks
This interface structures graphical data by separating color information (palette) from frame data (indices), optimizing memory usage and enabling easy manipulation of animation frames. The fixed size of 64 frames with 256 palette indices each suggests a design tailored for specific rendering constraints or legacy formats.


---

## GabrielSequenceMetadata
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

```typescript
export interface GabrielSequenceMetadata
```


GabrielSequenceMetadata is a compact data contract that captures meta-information about a Gabriel sequence generation. It includes the seed used to derive the sequence, the generation time as an ISO 8601 timestamp, and an optional human-readable summary of the sequence's current state. This metadata is typically attached alongside the sequence payload to support auditing, debugging, and reproducibility without exposing or transporting the full sequence data.

## Remarks
By separating provenance (seed, timestamp) and a compact state description from the sequence values, this interface enables lightweight logging and correlation across system boundaries. It facilitates deterministic reproduction and easier debugging: given the same seed and generation time, developers can trace how a sequence was produced, and the stateSummary provides a quick digest of progress or characteristics. It acts as a stable contract between producers and consumers of sequence data.

## Example
```typescript
const meta: GabrielSequenceMetadata = {
  seed: 42,
  generatedAt: new Date().toISOString(),
  stateSummary: 'initialized; ready to generate'
};
```

## Notes
- generatedAt should be a valid ISO 8601 timestamp; consider using toISOString()
- stateSummary may be null if no summary is available

---

## SequenceSource
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** type alias

```typescript
export type SequenceSource =
  |
```


SequenceSource is a discriminated union type that represents the origin or source of a sequence within the application. It allows developers to handle sequences differently based on whether they come from a conversation or potentially other sources, facilitating type-safe branching and processing of sequence data.

## Remarks
This type uses a discriminated union pattern with a `kind` property to distinguish between different sequence sources. This design enables clear and maintainable code paths when working with sequences originating from various contexts, such as conversations or other future extensions.

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


doFetch is a small helper that resolves the URL for a given SequenceSource and fetches it, returning a `Promise<Response>`. It centralizes the fetch logic for sequence-related endpoints by delegating URL resolution to urlFor(source) and by applying a consistent set of fetch options, namely credentials: 'include' and an optional AbortSignal.

## Remarks
doFetch sits at the boundary between high-level API usage and low-level network calls. It hides the mechanics of URL resolution and credential policy behind a single, reusable function, reducing duplication and ensuring that all requests for sequence data follow the same convention. It also provides a natural extension point: if you later need common headers, error handling, or retry behavior, you can add it in one place without touching every caller.

## Notes
- This function always uses credentials: 'include'; if your authentication strategy changes, you may need to adjust this behavior or bypass it in contexts that require different credentials handling.
- The AbortSignal is optional; callers that do not need cancellation can omit the argument.
- The function returns a native `Promise<Response>`; downstream code should inspect response.ok and parse the body as needed (e.g., via response.json()).

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


FetchGabrielSequence retrieves a GabrielSequence from the provided SequenceSource and abstracts away the boilerplate around authenticated requests. It first uses doFetch to fetch data, but if the server indicates the session is unauthorized (401), it attempts a session refresh via refreshSession and retries once. If the retry still results in 401, it signals that the session has expired and throws. On a successful response, it returns the parsed GabrielSequence.

## Remarks
This function centralizes session-refresh semantics for sequence endpoints. It coordinates doFetch, refreshSession, and session-expiry signaling, so callers don't have to reimplement common error handling for authentication. It serves as a robust single point of truth for fetching Gabriel sequences while preserving clear error pathways for failed or expired sessions.

## Example
```typescript
// Common usage pattern
try {
  const abort = new AbortController();
  const sequence = await fetchGabrielSequence(mySource, abort.signal);
  // Use the GabrielSequence object here
} catch (err) {
  // Handle fetch or authentication errors
}
```

## Notes
- A 401 response may trigger a session refresh; if the refresh fails or the retried fetch still returns 401, a "Session expired. Please sign in again." error is thrown.
- The function casts the response body to GabrielSequence; if the payload shape does not match, runtime typing may produce unexpected results.
- Passing an AbortSignal allows cancellation of the request; consider wiring UI controls to abort long-running fetches if needed.

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


Computes the API URL for the sequence resource associated with a given SequenceSource. If the source denotes a conversation, the returned path points to that conversation's sequence endpoint; otherwise it points to the project's sequence endpoint.

## Remarks
URL construction is centralized here to ensure consistent routing to sequence resources across the UI. It relies on a discriminated union (SequenceSource) and encodeURIComponent to safely embed IDs in the path, so callers don't need to assemble routes by hand.

## Example
```typescript
// Conversation sequence URL
urlFor({ kind: 'conversation', conversationId: 'abc-123' });
// -> '/api/conversations/abc-123/sequence'

// Project sequence URL
urlFor({ kind: 'project', projectId: 'proj 9' });
// -> '/api/projects/proj%209/sequence'
```

## Notes
- IDs are encoded with encodeURIComponent to handle special characters in path segments.
- Pass raw IDs (do not pre-encode) to avoid double-encoding; the function applies encoding itself.
- The function returns a relative path (no scheme or host); callers may prepend a base URL if needed.

---
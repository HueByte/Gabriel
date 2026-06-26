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

Represents a versioned, palette-based animation/sequence used by the Gabriel format: a palette of RGB colors, a fixed set of frames where each frame is an array of palette indices, and accompanying metadata. Reach for this interface when producing, serializing, or consuming the canonical Gabriel sequence payload in application code.

## Remarks
This interface defines a compact, palette-indexed representation for pixel sequences: the palette supplies RGB colors (as [r, g, b] triples) and each frame references those colors by index. The separate version number allows parsers to evolve the on-disk or on-wire format while preserving backward/forward compatibility, and metadata holds ancillary information (timestamps, author, playback hints, etc.) that is orthogonal to rendering data.

## Example
```typescript
// Construct a simple GabrielSequence with a 2-color palette and 64 frames of 256 indices.
const seq: GabrielSequence = {
  version: 1,
  palette: [
    [0, 0, 0],      // black
    [255, 255, 255] // white
  ],
  frames: Array.from({ length: 64 }, (_, i) =>
    // each frame is 256 palette indices; here we alternate colors per pixel
    Array.from({ length: 256 }, (_, p) => ((i + p) % 2 ? 1 : 0))
  ),
  // metadata shape is GabrielSequenceMetadata — fill according to that type
  metadata: {} as any
};
```

## Notes
- The implementation expects exactly 64 frames and each frame to contain exactly 256 indices; consumers/validators should enforce these lengths.
- Palette entries are [r, g, b] triples (typically 0–255). Palette indices in frames must be valid integer indexes into the palette array.
- Use the version number to gate parsing/compatibility logic; do not assume format details remain stable across versions.

---

## GabrielSequenceMetadata

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

A small object describing metadata for a generated Gabriel sequence — use this when returning or persisting information about how and when a sequence was produced (for reproducibility, auditing, or UI display).

## Remarks
This interface captures three minimal pieces of metadata needed to reproduce or inspect a generated sequence: a numeric seed (used for deterministic generation), a timestamp for when the sequence was generated (stored as an ISO 8601 string), and an optional human-readable summary of the generator's state. It is intended as a lightweight, transport-friendly DTO rather than a validation or parsing utility.

## Example
```typescript
// Create metadata for a newly generated sequence
const metadata: GabrielSequenceMetadata = {
  seed: 42,
  generatedAt: new Date().toISOString(), // ISO 8601 timestamp (UTC)
  stateSummary: "used-default-params"
};

// Parse the timestamp when needed
const generatedDate = new Date(metadata.generatedAt);
console.log(generatedDate.toUTCString());
```

## Notes
- generatedAt must be an ISO 8601 string; consumers should parse it with Date or a dedicated parser to handle timezones reliably.
- seed is typed as number but not constrained by the interface; if your application requires an integer range, validate it elsewhere.
- stateSummary may be null when no summary is available; treat it as nullable rather than optional.


---

## SequenceSource

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** type

Represents the origin/type of a sequence as a discriminated union. In the available source the only visible discriminant is { kind: 'conversation' }, so this type is used to tag sequences that originate from a conversation and to allow callers to narrow behavior by that tag.

## Remarks
This symbol is a discriminated (tagged) union keyed by the kind property so callers can perform type-safe narrowing (e.g., switch or if checks on source.kind) to handle different sequence origins differently. The provided source is truncated; additional variants and associated properties may exist in the full definition.

## Example
```typescript
function handleSource(source: SequenceSource) {
  if (source.kind === 'conversation') {
    // Narrowed to the conversation variant — handle conversation-origin sequences
    // (Inspect additional properties on `source` if present in the full type)
  } else {
    // Handle other kinds (other variants may exist in the full definition)
  }
}
```

## Notes
- The source code available for this type is incomplete and shows only the beginning of the union (the 'conversation' discriminant). Verify the full type before constructing values because other variants may require additional properties.
- Avoid assuming that { kind: 'conversation' } alone is a valid value; the conversation variant may include required fields not visible in the truncated snippet.

---

## doFetch

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Performs an HTTP fetch for a given SequenceSource by resolving its URL (via urlFor) and calling the browser fetch API with credentials included. Accepts an optional AbortSignal to allow the caller to cancel the request; returns the raw `Promise<Response>` so callers can inspect status, headers or parse the body as needed.

## Remarks
This small helper centralizes request options used by the sequence-related API surface: it ensures credentials (cookies / HTTP auth) are always sent and that callers can pass an AbortSignal for cancellation. It intentionally returns the unprocessed Response so higher-level functions can apply consistent error handling and body parsing policies.

## Example
```typescript
const controller = new AbortController();
// start a fetch for some SequenceSource
const respPromise = doFetch(mySequenceSource, controller.signal);

// optionally cancel if needed
// controller.abort();

const resp = await respPromise;
if (!resp.ok) {
  // handle non-2xx status
  throw new Error(`Request failed: ${resp.status}`);
}
const data = await resp.json();
```

## Notes
- The function does not treat non-2xx HTTP statuses as errors — fetch resolves successfully for those; callers must check Response.ok or status explicitly.
- fetch rejects only for network errors or when the signal is aborted; aborting will cause a rejected promise with an AbortError-like DOMException.
- credentials: 'include' sends cookies and auth headers even for cross-origin requests; ensure the server CORS and credential policies are compatible (e.g., Access-Control-Allow-Credentials).
- urlFor is used to derive the request URL; if it throws or returns an invalid URL, fetch will reject or throw accordingly.

---

## fetchGabrielSequence

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Fetches a GabrielSequence from the server for the given SequenceSource, handling authentication expiry by attempting a single session refresh. Use this helper when callers want a higher-level fetch that retries once on HTTP 401, signals a global session-expired condition if refresh fails, and throws on any non-success HTTP response.

## Remarks
This function centralizes common fetch behavior for sequence data: it delegates the actual network call to doFetch, attempts one session refresh on a 401 response, and converts a successful response body to the GabrielSequence type. It intentionally treats 401 specially (refresh-then-fail) so callers don't need to repeat session-refresh logic across the codebase.

## Example
```typescript
const controller = new AbortController();
try {
  const sequence = await fetchGabrielSequence(mySource, controller.signal);
  // use `sequence` here
} catch (err) {
  // handle errors: network failures, non-2xx responses, or session-expired
  if (err instanceof Error) {
    console.error('Failed to load sequence:', err.message);
  }
}
// To cancel:
controller.abort();
```

## Notes
- The function throws an Error for any non-ok HTTP response (after a single 401 refresh attempt). Callers must catch exceptions.
- A failed session refresh causes signalSessionExpired() to be invoked and then an Error('Session expired. Please sign in again.') is thrown; this may trigger global sign-in UX.
- Only a single retry is performed on 401. Other transient status codes are not retried automatically.
- The JSON body is cast to GabrielSequence without runtime validation; malformed responses can still produce runtime errors or invalid objects.
- The supplied AbortSignal is passed through to the underlying fetch call; canceling the signal will abort the request.

---

## urlFor

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Constructs the relative API path for the "sequence" resource based on a SequenceSource. Use this helper whenever you need the correct endpoint for a sequence tied to either a conversation or a project so callers don't have to duplicate path logic or worry about URL-encoding the identifier.

## Remarks
This function centralises the logic that differs between conversation-scoped and project-scoped sequence endpoints. It returns only the path portion (for example, "/api/conversations/.../sequence") and URL-encodes the conversation or project identifier to avoid invalid characters in the path.

## Example
```typescript
// For a conversation-backed sequence
const convPath = urlFor({ kind: 'conversation', conversationId: 'team/alpha#1' });
// convPath -> "/api/conversations/team%2Falpha%231/sequence"

// For a project-backed sequence
const projPath = urlFor({ kind: 'project', projectId: 'proj-123' });
// projPath -> "/api/projects/proj-123/sequence"
```

## Notes
- The function expects the provided SequenceSource to include the appropriate id field for its kind; if the id is missing the returned string will contain "undefined". Validate inputs before calling if necessary.
- It returns a relative path only — callers should prepend the API base URL or origin and may add query parameters or a trailing slash if required by the server.
- Identifiers are encoded with encodeURIComponent; this handles common unsafe characters but does not validate semantic correctness of the id.

---
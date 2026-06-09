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

A compact, indexed-color representation of an animated sequence: a versioned payload that contains a palette (RGB triplets), exactly 64 frames where each frame is an array of 256 palette indices, and associated metadata. Use this shape when producing or consuming sequences for the webapp API or any renderer that expects 256‑pixel frames indexed into a palette.

## Remarks
This interface encodes animations as an indexed (palette-based) format to keep payload size small and to separate color data from pixel indices. The palette stores colors as [r, g, b] arrays and the frames store indices into that palette; metadata (GabrielSequenceMetadata) carries descriptive or playback-related information. The version field lets consumers evolve the schema while keeping backward/forward compatibility checks simple.

## Example
```typescript
const seq: GabrielSequence = {
  version: 1,
  // palette: array of [r,g,b] values (0-255)
  palette: [
    [0, 0, 0],       // index 0 = black
    [255, 255, 255], // index 1 = white
    [255, 0, 0],     // index 2 = red
    // ... up to 256 entries
  ],
  // frames: 64 frames, each is an array of 256 indices into the palette
  frames: Array.from({ length: 64 }, () => new Array(256).fill(0)),
  metadata: {
    // fields defined by GabrielSequenceMetadata (e.g. title, author, timing)
  }
};
```

## Notes
- Each palette entry is an [r, g, b] triplet with values typically in the 0–255 range.
- frames must contain exactly 64 arrays, and each frame array must have exactly 256 numeric indices.
- Every index in frames must be a valid index into palette (an integer >= 0 and < palette.length).

---

## GabrielSequenceMetadata

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

Represents metadata produced alongside a Gabriel sequence: the numeric seed used for deterministic generation, an ISO 8601 timestamp indicating when the sequence was produced, and an optional short summary of the generator's state. Reach for this interface when storing, transmitting, or displaying the minimal reproducibility and audit information that accompanies a generated sequence.

## Remarks
This interface encapsulates a small, transport-friendly set of information required to reproduce or inspect a Gabriel-generated sequence. Keeping generatedAt as a plain ISO 8601 string makes the metadata JSON-serializable without Date objects, while seed provides the value needed to re-seed a deterministic generator. stateSummary is intended as an optional compact description or snapshot of the generator's internal state and may be consumed by humans or diagnostic tooling.

## Example
```typescript
// Creating metadata for a generated sequence
const metadata: GabrielSequenceMetadata = {
  seed: 12345,
  generatedAt: new Date().toISOString(),
  stateSummary: "rounds=5;checksum=af3b2"
};

// Parsing generatedAt for use as a Date
const generatedDate = new Date(metadata.generatedAt);
console.log(generatedDate.toUTCString());

// Handling nullable stateSummary
if (metadata.stateSummary) {
  console.log('State summary:', metadata.stateSummary);
} else {
  console.log('No state summary provided');
}
```

## Notes
- generatedAt is a string (ISO 8601) rather than a Date object — callers should parse it (e.g., new Date(...)) before performing date operations.
- seed is typed as number but is intended to be used as an integer seed; callers should ensure integer semantics if required by the generator.
- stateSummary may be null when no summary is available; consumers should handle the nullable case explicitly.

---

## SequenceSource

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** type

Represents the origin of a sequence using a discriminated-union pattern. The union is distinguished by a required `kind` property (e.g. 'conversation' in the visible fragment); use this type when code needs to carry, inspect or branch logic based on where a sequence came from instead of passing unstructured strings or multiple optional fields.

## Remarks
This abstraction centralizes different origin types behind a single type-safe union so callers can perform exhaustive, kind-based handling (switch/narrow) and the compiler will catch missing cases. It is intended for use wherever sequence-processing logic must behave differently depending on the source — for routing, display, or conditional processing — and when you want to serialize the origin in a stable, explicit shape.

## Example
```typescript
function handleSequence(src: SequenceSource) {
  switch (src.kind) {
    case 'conversation':
      // narrow to the conversation variant; safe to access conversation-specific fields
      // e.g. src.conversationId or src.participant
      break;
    // add other cases for the union's kinds here
    default: {
      // ensure exhaustive checking
      const _exhaustive: never = src;
      return _exhaustive;
    }
  }
}
```

## Notes
- Always narrow on the `kind` property before accessing variant-specific fields; do not assume other properties exist.
- Prefer exhaustive switches or a `never`-check to catch new variants when the union is extended.
- When serializing/deserializing, ensure the `kind` field is preserved so runtime narrowing remains possible.


---

## doFetch

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Builds and sends an HTTP request for a sequence resource identified by the provided SequenceSource, returning the raw fetch Response. The URL is produced by urlFor(source); the request always includes credentials ('include') and accepts an optional AbortSignal to cancel the request. Use this helper when you want a consistent, credentialed fetch for sequence endpoints and intend to handle the Response (status checks, parsing, errors) at the call site.

## Remarks
This small wrapper centralizes how sequence-related endpoints are contacted: it encapsulates URL construction (via urlFor) and a fixed fetch option set (credentials included, optional cancellation). It intentionally returns the raw `Promise<Response>` so callers can implement their own status handling, parsing, and error semantics rather than imposing a single behavior here.

## Example
```typescript
// Abortable fetch for a sequence resource
const controller = new AbortController();
const signal = controller.signal;

// assume `source` is a SequenceSource value
doFetch(source, signal)
  .then(response => {
    if (!response.ok) throw new Error(`Fetch failed: ${response.status}`);
    return response.json();
  })
  .then(data => {
    // handle parsed data
  })
  .catch(err => {
    // handle network errors or abort
  });

// to cancel:
// controller.abort();
```

## Notes
- The function does not check response.ok or parse the response body; callers must inspect the Response and handle non-2xx statuses.
- If the provided AbortSignal is triggered (or a network error occurs), fetch will reject; callers should handle rejections accordingly.
- Credentials are always sent ('include'), so cookies and authentication headers will be attached — be mindful of cross-origin implications and security/privacy concerns.
- The URL is produced by urlFor(source); any errors thrown by urlFor will propagate to the caller.

---

## fetchGabrielSequence

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Fetches a GabrielSequence from the server for the given SequenceSource. Uses an optional AbortSignal to allow cancellation. If the initial request returns 401 Unauthorized the function will attempt to refresh the session and retry once; if the retry also returns 401 it signals session expiry and throws. Any non-OK response results in an Error that includes the HTTP status and statusText; on success the response body is parsed as JSON and returned as a GabrielSequence.

## Remarks
This function centralizes the network and authentication handling for retrieving Gabriel sequences: it delegates the actual network request to doFetch, handles an automatic single refresh-on-401 retry via refreshSession, and calls signalSessionExpired when re-authentication is required. Callers benefit from the built-in retry/expiry signaling and only need to handle the parsed GabrielSequence or errors.

## Example
```typescript
const controller = new AbortController();
try {
  const sequence = await fetchGabrielSequence(mySource, controller.signal);
  // use sequence
} catch (err) {
  // handle network errors, authentication expiration, or other failures
  console.error(err);
}
```

## Notes
- The function performs at most one automatic retry after a 401; if the retry also yields 401 it signals session expiry and throws.
- The JSON body is cast to GabrielSequence without runtime shape validation; callers should not assume validation beyond the server's response.
- Passing an AbortSignal will cancel the underlying request; cancellation causes the returned promise to reject (e.g., with a DOMException), which should be handled by the caller.

---

## urlFor

> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** function

Returns a relative API path for the sequence endpoint corresponding to the provided SequenceSource. Use this helper whenever code needs the correct endpoint for either a conversation-scoped sequence or a project-scoped sequence instead of building the string inline.

## Remarks
Centralizes URL construction for sequence-related API calls and ensures identifiers are safely encoded with encodeURIComponent. The function chooses the conversation route when source.kind === 'conversation'; for any other kind it falls back to the project route, so callers should provide a SequenceSource that matches one of the expected shapes. The returned string is a relative path (starts with "/api/...") — callers should prepend a host/origin if an absolute URL is required.

## Example
```typescript
// conversation source
const convSrc = { kind: 'conversation', conversationId: 'room/123' } as SequenceSource;
console.log(urlFor(convSrc));
// Output: "/api/conversations/room%2F123/sequence"

// project source
const projSrc = { kind: 'project', projectId: 'proj:alpha' } as SequenceSource;
console.log(urlFor(projSrc));
// Output: "/api/projects/proj%3Aalpha/sequence"
```

## Notes
- The function encodes the ID with encodeURIComponent to make the URL safe for IDs containing slashes, colons, spaces, etc.
- If SequenceSource can have additional kinds beyond 'conversation' and 'project', this implementation will treat any non-'conversation' kind as a project; ensure callers pass the expected shape.
- Returns a relative path only; do not assume it includes hostname or protocol.

---
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


Represents the shape of a Gabriel sequence used by the web application to encode an animation as palette-indexed frames. A GabrielSequence carries a version number, a color palette, a collection of frames, and sequence metadata. The palette is an array of RGB triples [r, g, b], while frames is 64 arrays of 256 palette indices each. The metadata field carries additional information described by GabrielSequenceMetadata. This interface is used whenever a Gabriel sequence must be created, transmitted, or consumed by rendering or export tooling, providing a stable contract between producers and consumers of sequence data while avoiding per-frame color data duplication.

## Remarks
GabrielSequence acts as a compact data contract that enables efficient interchange and shared color data across frames. By separating the palette from the per-frame indices, it supports lightweight serialization and consistent color interpretation across components that render or export animations. It relies on GabrielSequenceMetadata to convey context (such as descriptive details about the sequence) without embedding that metadata into every frame. This separation also makes it easy to swap palettes or reuse them across frames without modifying the frame indices.

## Notes
- The frames field is documented as 64 frames; ensure the array length is exactly 64 and each frame contains 256 palette indices.
- Indices in frames must be valid indices into the provided palette (0 <= index < palette.length) to ensure correct color mapping.

---

## GabrielSequenceMetadata
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** interface

```typescript
export interface GabrielSequenceMetadata
```


GabrielSequenceMetadata is a compact data contract that describes the metadata for a Gabriel sequence. It captures the seed used to generate the sequence, the generation timestamp in ISO 8601 format, and an optional summary of the sequence's current state.

## Remarks
GabrielSequenceMetadata serves as a stable boundary for metadata across modules that handle Gabriel sequences. By keeping seed, generatedAt, and stateSummary as a simple, serializable payload, it helps decouple metadata concerns from the sequence data and supports consistent logging, auditing, and client display. The nullability of stateSummary communicates that a descriptive state may be unavailable, which consumers should handle gracefully.

## Example
```typescript
const exampleMetadata: GabrielSequenceMetadata = {
  seed: 98765,
  generatedAt: new Date('2024-01-01T12:00:00Z').toISOString(),
  stateSummary: "Initial seed applied; ready to generate"
};
```

## Notes
- If stateSummary can be null, code consuming the metadata should guard for null before string operations or display.
- generatedAt should reflect the exact time of generation; ensure that time is captured in UTC and serialized in ISO 8601 format.

---

## SequenceSource
> **File:** `src/webapp/src/api/sequence.ts`  
> **Kind:** type alias

```typescript
export type SequenceSource =
  | { kind: 'conversation'; conversationId: string }
  | { kind: 'project'; projectId: string };
```


SequenceSource represents the origin of a sequence in the API: it is a discriminated union that can be either a conversation-based source or a project-based source, each carrying its own identifier. This type makes explicit how a sequence was produced or should be retrieved, enabling safe handling by inspecting the kind and then accessing the corresponding id.

## Remarks
Because it is a discriminated union on the 'kind' field, TypeScript can narrow the type in branches, allowing safe access to either conversationId or projectId without risk of reading a non-existent property.

## Example
```ts
function logSequenceSource(src: SequenceSource): void {
  switch (src.kind) {
    case 'conversation':
      console.log(`Conversation sequence: ${src.conversationId}`);
      break;
    case 'project':
      console.log(`Project sequence: ${src.projectId}`);
      break;
  }
}
```

## Notes
- The two variants are mutually exclusive; only one id property exists for any given value.
- Access to the id field is only valid after narrowing by kind; attempting to read src.conversationId when kind is 'project' will be a TypeScript error.
- This type does not carry any additional data beyond the id fields; extend only if you need more variants.

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


Fetch wrapper that retrieves a Response for a given SequenceSource by issuing a request to the URL produced by urlFor(source). It returns the raw fetch `Promise<Response>`, leaving status checks, error handling, and body parsing to the caller. The request includes credentials: 'include' to send cookies and authentication data, and it forwards an optional AbortSignal to support cancellation from the caller.

## Remarks
This small abstraction centralizes how sequence data is fetched, keeping URL construction and fetch configuration aligned across consumers. It relies on urlFor(source) to determine the destination URL, and it ensures that cookies and credentials accompany the request, while allowing callers to cancel in-flight requests via AbortSignal. It does not perform error handling, retry logic, or response parsing.

## Notes
- Using credentials: 'include' sends cookies and HTTP authentication data; ensure your server's CORS policy allows credentials from the origin.
- The function does not parse the response or perform retries; consumers must check response.ok and parse the body as needed. If an AbortSignal is provided and the request is canceled, the Promise rejects with an AbortError.

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


Fetches a GabrielSequence for the given SequenceSource, optionally honoring an AbortSignal to cancel the request. It starts by performing a fetch via doFetch and, if the server replies with 401, attempts a session refresh and retries once. If the retry also yields 401, it signals that the session has expired and throws a clear error instructing the user to sign in again. For any non-ok response, it throws a descriptive error including the status code and status text. On success, the response body is parsed as a GabrielSequence and returned.

## Remarks
Encapsulates the authentication-flow and error handling for sequence retrieval, so callers don't need to implement retry-on-401 logic themselves. It centralizes the session-expiration behavior by signaling the session expiry when a refresh still fails, providing a single, consistent contract for sequence fetches across the app. It relies on shared helpers (doFetch, refreshSession, signalSessionExpired) to compose the observable behavior.

## Example
```ts
const source: SequenceSource = /* obtain from your app */;
const controller = new AbortController();
const sequence: GabrielSequence = await fetchGabrielSequence(source, controller.signal);
```

## Notes
- Only a single retry is attempted after a 401 response.
- If the session refresh fails or if the second fetch returns a non-OK status, a descriptive Error is thrown.
- The AbortSignal is passed through to allow cancellation of the underlying fetch.

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


urlFor takes a SequenceSource and returns the REST path to fetch its sequence. It selects between the conversation endpoint and the project endpoint based on source.kind, so callers pass a single SequenceSource and need not assemble the URL themselves.

## Remarks
urlFor centralizes endpoint construction for sequence retrieval. It ensures IDs are URL-encoded and selects the correct API path based on the source kind, reducing duplication and making future endpoint changes easier to adopt.

## Example
```ts
// Example usage
const convoSource = { kind: 'conversation', conversationId: 'c123' } as const;
const pathA = urlFor(convoSource);
// -> "/api/conversations/c123/sequence"

const projSource = { kind: 'project', projectId: 'p456' } as const;
const pathB = urlFor(projSource);
// -> "/api/projects/p456/sequence"
```

## Notes
- Be aware that non-'conversation' kinds default to the project path due to the ternary; ensure SequenceSource covers all valid kinds to avoid routing to the wrong endpoint.

---
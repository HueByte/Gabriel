# GabrielSequenceResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`

## Contents

- [GabrielSequenceResponse](#gabrielsequenceresponse)
- [SequenceMetadataResponse](#sequencemetadataresponse)

---

## GabrielSequenceResponse
> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

```csharp
public record GabrielSequenceResponse(
    int Version,
    IReadOnlyList<int[]> Palette,
    IReadOnlyList<int[]> Frames,
    SequenceMetadataResponse Metadata)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Version` | `int` | — |
| `Palette` | `IReadOnlyList<int[]>` | — |
| `Frames` | `IReadOnlyList<int[]>` | — |
| `Metadata` | `SequenceMetadataResponse` | — |


GabrielSequenceResponse is the wire-format container for a Gabriel sequence used by the Gabriel API surface. It bundles a Version, a Palette, a collection of Frames, and related Metadata. The frame data are 16×16 images encoded as palette indices, so the body remains compact: a 16×16 frame is 256 integers before any serialization overhead. Palette colors are serialized as [r, g, b] triples, and clients reconstruct full colors by indexing into Palette. Frames are transmitted in canonical order 0..63; to derive per-layer slices you can compute floor(frameIndex / 16), mapping 0 to DNA, 1 to Traits, 2 to Context, and 3 to Live.

## Remarks
GabrielSequenceResponse serves as a compact transport DTO that carries all data needed to render a Gabriel sequence: version, palette, frames, and metadata. By encoding pixel data as palette indices and storing palette colors separately, it minimizes payload size while preserving full color fidelity via a simple index look-up at decode time. The canonical 0..63 frame ordering enables straightforward layer extraction (DNA, Traits, Context, Live) by computing floor(frameIndex / 16).

## Notes
- Each frame is a 16×16 grid encoded as 256 integers; ensure the Frames collection contains exactly 64 frames (indices 0..63), each with 256 entries.
- Indices in frames must be in the range 0..Palette.Length-1; out-of-range indices cannot be decoded.
- Canonical ordering means you should not assume any other ordering; the layer derivation uses floor(frameIndex/16) to map to DNA/Traits/Context/Live.

---

## SequenceMetadataResponse
> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

```csharp
public record SequenceMetadataResponse(
    long Seed,
    DateTimeOffset GeneratedAt,
    string? StateSummary)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Seed` | `long` | — |
| `GeneratedAt` | `DateTimeOffset` | — |
| `StateSummary` | `string?` | — |


SequenceMetadataResponse is an immutable data transfer object that bundles the seed used to initialize a deterministic sequence, the exact time the metadata was generated, and an optional summary of the current state. It is intended for API responses that expose sequence metadata so clients can log, display, or reproduce results by using the seed and GeneratedAt timestamp; the StateSummary provides contextual prose when available.

## Remarks
By leveraging C#'s record semantics, this type guarantees value-based equality and immutability, aiding caching and comparison. The GeneratedAt field uses DateTimeOffset to preserve the original offset, avoiding timezone ambiguity in distributed systems. The optional StateSummary keeps the surface area small while still offering guidance for debugging or user-facing messages.

## Example
```csharp
// Example: constructing a metadata response
var metadata = new SequenceMetadataResponse(
    Seed: 1234567890L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "Initialized for deterministic run"
);
```

## Notes
- StateSummary may be null; callers should handle nulls gracefully.
- The Seed value is intended for reproducibility, not security; do not rely on it for cryptographic purposes.


---
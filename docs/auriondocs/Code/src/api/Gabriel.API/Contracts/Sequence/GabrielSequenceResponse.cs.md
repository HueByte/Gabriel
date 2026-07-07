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


GabrielSequenceResponse is the wire-format representation of a Gabriel sequence returned by the API. It carries a Version, a Palette of RGB colors, a sequence of Frames, and associated Metadata. To minimize bandwidth, frame data is stored as palette indices rather than raw RGB values: each frame corresponds to a 16×16 grid of pixels, with every entry in the grid being an index into Palette. Palette entries are serialized as [r, g, b] triples, and clients reconstruct RGB colors by looking up Palette by index. Frames are transmitted in canonical order from 0 to 63; the layer a given frame belongs to can be derived as floor(frameIndex / 16), mapping to 0=DNA, 1=Traits, 2=Context, 3=Live. The Version field enables evolution of the format, and Metadata (SequenceMetadataResponse) provides descriptive information about the sequence.

## Remarks

The abstraction separates color data (Palette) from spatial data (Frames), enabling compact wire-serialization and efficient caching. By encoding frames as palette indices, clients can reuse shared colors across frames and reduce payload size. The canonical frame ordering and layer derivation make it straightforward to render each frame and overlay or animate the Gabriel sequence by layer.

## Notes

- Indices into Palette must be valid (0 <= index < Palette.Count); out-of-range values indicate invalid data.
- Palette entries should be 3-length [r, g, b] triples with each component typically in the 0..255 range; clients should validate or clamp as needed.
- Frames are expected to comprise 64 frames in canonical order (0..63); rendering logic should rely on that ordering for correct layer composition.

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


Represents the metadata for a generated sequence. It encapsulates the 64-bit Seed used for deterministic generation, the exact timestamp of generation (GeneratedAt) with offset information, and an optional StateSummary describing the sequence's state at that moment. As an immutable record, SequenceMetadataResponse is a lightweight, transport-friendly payload suitable for API responses and inter-service messages where reproducibility and timing context are important.

## Remarks
SequenceMetadataResponse is a focused value object that bundles generation metadata into a single return type, enabling predictable transport across API boundaries. It preserves reproducibility via Seed, while GeneratedAt (DateTimeOffset) maintains the exact generation moment with its offset; StateSummary offers an optional, human-friendly description of the sequence state.

## Notes
- StateSummary is nullable; handle null gracefully.
- GeneratedAt uses DateTimeOffset; display with offset or convert to local time as needed.
- Seed is 64-bit; ensure clients do not lose precision in environments without 64-bit integer support.

---
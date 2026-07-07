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


GabrielSequenceResponse is the wire-format container for a Gabriel sequence. It groups a Version, Palette, Frames, and Metadata into a transportable payload. The Palette stores colors as [r, g, b] triples and Frames are sequences of palette indices that reference that palette. Clients reconstruct colors by looking up the palette by index and render each 16×16 frame accordingly. Frames are sent in canonical 0..63 order, and the layer for a given frame is determined by floor(frameIndex / 16) (0 = DNA, 1 = Traits, 2 = Context, 3 = Live). This structure keeps the on-wire size small by separating color data from per-frame indices, and it is designed for efficient transmission and decoding in client code.

## Remarks
GabrielSequenceResponse serves as an on-wire envelope for the Gabriel sequence concept. By separating palette from frame data, it minimizes duplication and supports caching and streaming across many frames. The metadata attaches contextual information about the sequence, enabling consumers to interpret and display the data consistently without duplicating domain knowledge in the frames themselves.

## Notes
- Palette indices in Frames must be valid against the provided Palette (0 <= index < Palette.Count).
- Frames are delivered in canonical order 0..63; preserve this order during serialization and deserialization.
- The Version field is a protocol-version marker; consumers should handle unknown versions gracefully (e.g., via future-compatible fallbacks).

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


SequenceMetadataResponse is an immutable data carrier that captures metadata about a Gabriel sequence run. It bundles a seed (Seed) used to initialize deterministic generation, the moment it was generated (GeneratedAt), and an optional human-friendly snapshot of the current state (StateSummary). Use this type when API responses or internal data paths need to convey reproducibility information and timing without embedding any behavior.

## Remarks
Because it is declared as a record, the type provides value-based equality and immutability, making it safer to pass around across layers and cache. The StateSummary field offers a lightweight, readable description of the sequence state without exposing internal details, which helps with debugging and auditing while keeping the payload small. This abstraction fits as a simple transfer object between server and client or between services, separating metadata concerns from the actual sequence logic.

## Example
```csharp
var metadata = new SequenceMetadataResponse(
    Seed: 42L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "Initialized"
);

// Accessors
Console.WriteLine(metadata.Seed);
Console.WriteLine(metadata.GeneratedAt);

// Deconstruct
var (seed, generatedAt, summary) = metadata;
```

## Notes
- StateSummary may be null; callers should handle null.
- Deconstruction is supported because the record is defined with positional parameters.

---
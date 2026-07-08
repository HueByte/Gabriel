# GabrielSequence.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`

## Contents

- [GabrielSequence](#gabrielsequence)
- [SequenceMetadata](#sequencemetadata)

---

## GabrielSequence
> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

```csharp
public sealed record GabrielSequence(
    int Version,
    Palette Palette,
    IReadOnlyList<Frame> Frames,
    SequenceMetadata Metadata)
{
    public const int FrameCount = 64;
    public const int SchemaVersion = 1;

    public Frame this[int index] => Frames[index];
    public Frame this[FrameLayer layer, int frameInLayer]
        => Frames[(int)layer * FrameLayers.FramesPerLayer + frameInLayer];
}
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Version` | `int` | — |
| `Palette` | `Palette` | — |
| `Frames` | `IReadOnlyList<Frame>` | — |
| `Metadata` | `SequenceMetadata` | — |


GabrielSequence is an immutable container that represents a 64-frame, palette-indexed sequence of 16×16 frames along with a shared Palette and metadata. It does not persist rendered bytes; instead, the sequence is generated on-demand from a seed and ConversationState by IGabrielSequenceGenerator, allowing rendering without storing heavy raster data.

## Remarks
GabrielSequence provides a stable, serializable contract for a composite animation. It separates generation from storage, enabling the engine to reconstruct frames on demand from the current conversation context while keeping a lightweight in-memory record of the sequence's structure. Frames are exposed through two access patterns: a simple int indexer for linear frame order and a two-argument indexer that maps to per-layer framing using FrameLayers.FramesPerLayer. The latter relies on the shared per-layer layout to support efficient per-layer rendering workflows.

## Notes
- The Frames collection must contain exactly FrameCount (64) frames.  
- Out-of-range access will throw the standard indexing exceptions if the computed index is outside the Frames list. 
- The (FrameLayer, int) indexer relies on FrameLayers.FramesPerLayer and the total layout; ensure these values align with your data to avoid misaddressing frames.

---

## SequenceMetadata
> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

```csharp
public sealed record SequenceMetadata(
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


SequenceMetadata is an immutable value object that captures the seed used for sequence generation, when the sequence was produced, and an optional human-readable summary of the live state that drove generation. It provides a compact bundle of inputs so callers can reproduce, debug, and trace a generated sequence.

## Remarks
It groups generation inputs into a single, immutable token that downstream components can store or compare. Being a sealed record provides value-based equality and guarantees immutability, which makes it reliable to pass around without risk of mutation. The StateSummary field is optional and intended for quick human-readable context used during debugging and observation passes.

## Example
```csharp
var metadata = new SequenceMetadata(
    Seed: 1234567890L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "playful, short-message rhythm, 4 turns in"
);
```

## Notes
- StateSummary is nullable; handle nulls gracefully if you rely on it for logic.
- Because SequenceMetadata is a record, equality is value-based: two instances are equal when all three properties match.

---
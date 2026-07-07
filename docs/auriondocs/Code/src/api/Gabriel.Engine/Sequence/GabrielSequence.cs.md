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


GabrielSequence is an immutable, on-demand representation of the canonical Gabriel animation sequence. It packages 64 palette-indexed frames (16×16), a shared Palette, and associated metadata into a single, versioned object without persisting rendered pixels. Sequences are produced on demand from a seed and ConversationState by IGabrielSequenceGenerator — inputs that already live on the Conversation entity — making GabrielSequence a lightweight, queryable view into the final frames and metadata. Frames can be accessed either by a simple 0-based index via sequence[index] or by specifying a layer and position within that layer via sequence[layer, frameInLayer].

## Remarks
GabrielSequence serves as the boundary between sequence generation and consumption. By keeping the actual pixel data out of persistence, it enables efficient recomputation and sharing of identical sequences across conversations. The Version and SchemaVersion fields help readers understand compatibility with the generator and storage format; the dual indexers map 2D frame layout into the underlying linear Frames collection, leveraging FrameLayers.FramesPerLayer to compute offsets.

## Notes
- The second indexer relies on FrameLayers.FramesPerLayer matching the layout of Frames; inconsistency can lead to out-of-bounds access.
- Ensure Frames.Length == FrameCount; otherwise indexers may throw.

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


SequenceMetadata is an immutable data carrier that encapsulates provenance information for a generated sequence. It stores the seed used to drive deterministic generation, the exact moment of generation, and an optional human-readable snapshot of the live state that influenced the result. This metadata enables reproducibility, debugging, and AI-assisted observation by providing context without altering the generated data.

## Remarks
This record exists to separate concerns between the generated sequence and its provenance, enabling logs, replay, and diagnostics across the generation pipeline. The value-based equality and immutability of a record make it a natural wrapper for provenance, allowing it to be compared, serialized, and passed through APIs without mutation. It fits alongside the sequence data without imposing behavior on generation, serving as a lightweight context carrier for diagnostics and reproducibility.

## Example
```csharp
var metadata = new SequenceMetadata(
    Seed: 42L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "calm, steady rhythm; 4 turns in"
);
```

## Notes
- StateSummary is optional; pass null if no human-readable context is available or desired.
- Seed represents deterministic input for generation; different seeds yield different results and support replay or auditing of outcomes.
- GeneratedAt should reflect the precise time of generation to enable accurate ordering and traceability in logs and analyses.

---
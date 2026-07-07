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


GabrielSequence is an immutable record that encapsulates the data composing a canonical Gabriel animation sequence: a version, a shared Palette, a 64-frame collection, and associated metadata. It is the in-memory, render-ready representation used by the emotion engine; rendered bytes are not persisted. Sequences are generated on-demand from a seed and the ConversationState by IGabrielSequenceGenerator, with both inputs available on the Conversation entity. Frames can be accessed either by a flat index (0..63) via the single-parameter indexer, or by a (FrameLayer, frameInLayer) pair via the two-parameter indexer, which maps into the underlying Frames list using the layer offset (FrameLayers.FramesPerLayer).

## Remarks
GabrielSequence exists to separate the persistence model from the rendered output, enabling reproducible on-demand rendering while preserving a simple, immutable representation. The explicit FrameCount and SchemaVersion document expectations for downstream components and serializers, and the two indexers provide convenient access patterns: a straightforward, linear read by index for sequence-wide operations, and a layer-aware read for scenarios that align with the frame-layer organization of the source media. This separation also makes equality comparisons meaningful for caching or deduplication since the sequence’s content is the combination of Version, Palette, Frames, and Metadata.

## Example
```csharp
GabrielSequence seq = GetGabrielSequence(seed, convState);
Frame first = seq[0];
FrameLayer layer = default; // choose appropriate layer depending on usage
Frame firstInLayer = seq[layer, 0];
```

## Notes
- Rendered bytes are not persisted; regenerating frames requires access to the seed and ConversationState used by the generator.
- Access by layer assumes a consistent organization of frames per layer (FrameLayers.FramesPerLayer); changing that constant would affect indexing.
- GabrielSequence is a record, so it is immutable; to reflect changes you would construct a new instance with updated data.

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


SequenceMetadata is a compact, immutable record that captures the metadata produced alongside a generated sequence. It records the Seed used to drive generation, the precise GeneratedAt timestamp, and an optional human-readable StateSummary describing the live state that influenced the result. Use this type when you need reproducibility, debugging context, or to provide contextual notes for the observation pass during AI-assisted analysis.

## Remarks

By encapsulating seed, time, and an optional state summary, SequenceMetadata serves as a stable contract between the generation logic and tooling that inspects or replays sequences. As a value-based record, it participates in equality checks naturally and can be serialized alongside outputs. The presence of Seed and GeneratedAt enables deterministic replay and traceability across components in Gabriel.Engine’s sequence pipeline, while StateSummary offers debugging breadcrumbs without encoding the entire live state.

It can be attached to generated outputs to preserve provenance across storage or transport.

## Notes

- StateSummary may be null — guard against null before using it.
- Seed and GeneratedAt are provenance data; preserve them when passing results through layers.
- SequenceMetadata is immutable (record); to "modify" values you must construct a new instance.

---
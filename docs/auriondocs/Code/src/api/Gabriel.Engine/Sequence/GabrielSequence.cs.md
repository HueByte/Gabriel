# GabrielSequence.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`

## Contents

- [GabrielSequence](#gabrielsequence)
- [SequenceMetadata](#sequencemetadata)

---

## GabrielSequence

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

Represents the canonical Gabriel Sequence: a runtime, in-memory container for the sequence's shared palette, 64 palette-indexed 16×16 frames, and associated metadata. Reach for this record when you need the generated sequence object attached to a Conversation (it is produced on-demand by the sequence generator) rather than a persisted image or rendered bytes.

## Remarks
This record models the canonical, schema-versioned sequence; it intentionally does not persist rendered bytes — sequences are constructed from the conversation seed and state by IGabrielSequenceGenerator. Two constants surface important invariants: FrameCount (64) is the expected number of frames, and SchemaVersion identifies the sequence schema for compatibility checks. The two indexers provide convenient access either by flat frame index or by (layer, frame-in-layer) using FrameLayers.FramesPerLayer to map layers into the flat Frames list.

## Example
```csharp
// Access by flat index
var firstFrame = sequence[0];

// Access by layer + frame-in-layer
FrameLayer layer = /* obtain or compute layer */ (FrameLayer)1;
int frameInLayer = 3;
var frame = sequence[layer, frameInLayer];
```

## Notes
- The Frames list is expected to contain exactly FrameCount (64) entries; callers should validate this when constructing or consuming sequences.
- Both indexers will throw the usual collection index errors if an out-of-range index or frameInLayer is supplied; ensure indices are validated before access.
- The two-argument indexer computes the flat index as (int)layer * FrameLayers.FramesPerLayer + frameInLayer, so correct FrameLayers.FramesPerLayer values are required for proper mapping.

---

## SequenceMetadata

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

A small immutable container holding provenance and debugging information for a generated sequence. Use this to record the deterministic seed used during generation, the timestamp when the sequence was produced, and an optional short human‑readable summary of the live state that drove the generation (useful for observation, debugging and replay).

## Remarks
This record exists to make sequence generation reproducible and inspectable. The Seed captures the source of randomness so the same sequence can be re-generated when needed; GeneratedAt timestamps the exact generation moment (including offset) for audit and ordering; StateSummary provides a compact, human-friendly description of the live state that influenced generation and is intended for debugging and for any observation-style passes by other systems.

## Example
```csharp
var meta = new SequenceMetadata(
    Seed: 1234567890L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "playful, short-message rhythm, 4 turns in"
);

// Records are immutable and support value equality and with-expressions:
var later = meta with { GeneratedAt = DateTimeOffset.UtcNow.AddSeconds(30) };
```

## Notes
- StateSummary is nullable; prefer supplying a short summary when available to aid debugging and human inspection.
- GeneratedAt is a DateTimeOffset so the offset is preserved; using UTC (DateTimeOffset.UtcNow) avoids ambiguity across systems.
- Seed is a long and may be used to re-seed any PRNG for deterministic replay; semantics of negativity depend on the consumer.
- This is an immutable record: instances compare by value and can be copied with the with expression.

---
# GabrielSequence.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`

## Contents

- [GabrielSequence](#gabrielsequence)
- [SequenceMetadata](#sequencemetadata)

---

## GabrielSequence

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

Represents the canonical Gabriel sequence: a fixed collection of 64 palette-indexed 16×16 frames together with a shared Palette and sequence-level metadata. Use this record when working with an in-memory sequence produced by the generator (IGabrielSequenceGenerator) or when passing sequence data through the engine; it is not responsible for persisting rendered bytes.

## Remarks
This record is a compact, value-like container for the engine's canonical sequence format. It centralizes constants (FrameCount and SchemaVersion) and provides convenient indexers for accessing frames either by absolute index or by (layer, frame-in-layer). The design keeps sequences as regenerated, ephemeral artifacts (the file-level comment indicates rendering bytes are not persisted), so GabrielSequence is intended for transient in-memory use and interoperability between the generator, renderer, and any consumers that need frame, palette or metadata access.

## Example
```csharp
// Constructing (most callers receive a generated instance rather than creating one manually)
var sequence = new GabrielSequence(
    Version: GabrielSequence.SchemaVersion,
    Palette: palette,
    Frames: framesList, // must contain exactly GabrielSequence.FrameCount items
    Metadata: metadata);

// Access by absolute index
var first = sequence[0];

// Access by layer and frame-in-layer (FrameLayer is an enum of layers)
var frame = sequence[FrameLayer.Base, 3];
```

## Notes
- The Frames list is expected to contain exactly GabrielSequence.FrameCount (64) items; callers should validate the count before constructing or consuming a sequence.
- Both indexers delegate to the underlying IReadOnlyList and will throw the usual IndexOutOfRangeException for invalid indices; the layered indexer computes an offset using FrameLayers.FramesPerLayer.
- The record provides shallow immutability: the GabrielSequence instance is immutable, but the IReadOnlyList and the Frame/Palette/Metadata objects it references may be mutable and are not protected for thread-safety.

---

## SequenceMetadata

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequence.cs`  
> **Kind:** record

Holds small, audit-and-reproducibility-focused metadata for a generated sequence: the numeric Seed used to drive any deterministic generation, the timestamp when the sequence was produced, and an optional short human-readable StateSummary that describes the live state that produced the sequence (intended for debugging and human inspection).

## Remarks
SequenceMetadata exists to carry the minimal information needed to reproduce and understand a generated sequence. Seed allows a compatible generator to recreate the same output when the same algorithm is applied; GeneratedAt records when the sequence was produced for ordering and auditing; StateSummary provides a compact, human-oriented description of the live state that led to generation (not a machine-readable snapshot). Because this is a positional record it has value equality and is convenient for logging, persistence, and comparisons in tests.

## Example
```csharp
// Create metadata when producing a sequence
var meta = new SequenceMetadata(
    Seed: 42L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "playful, short-message rhythm, 4 turns in");

// Read values (deconstruction or properties)
var (seed, generatedAt, summary) = meta;
Console.WriteLine($"Seed={seed}, GeneratedAt={generatedAt}, Summary={summary}");
```

## Notes
- GeneratedAt is a DateTimeOffset but the code does not enforce a specific timezone; prefer using a consistent timezone (e.g., UTC) when creating and comparing timestamps.
- StateSummary is nullable and intended to be a short, human-friendly string; do not rely on it for programmatic decisions or parsing.
- Seed is for reproducibility only: to recreate the same sequence you must use the same generation algorithm and consume the seed in the same way. Casting the 64-bit Seed to a 32-bit int (for APIs that expect int) will lose information and may break reproducibility.
- The record is immutable (positional record) and supports value equality, which makes it suitable for logging, persistence, and equality checks in tests.
```

---
# GabrielSequenceResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`

## Contents

- [GabrielSequenceResponse](#gabrielsequenceresponse)
- [SequenceMetadataResponse](#sequencemetadataresponse)

---

## GabrielSequenceResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

A compact, wire-format representation of a multi-layer Gabriel sequence intended for API exchange. Use this type when serializing or deserializing the full sequence (palette + frames + metadata) between client and server, especially when bandwidth or payload size matters and frames are palette-indexed rather than raw RGB.

## Remarks
This record encodes frames as palette indices rather than full RGB tuples to keep the payload small: each 16×16 frame is 256 bytes pre-serialization versus several kilobytes for raw RGB. Palette entries are serialized as [r, g, b] integer triples and clients reconstruct concrete color values by indexing into the Palette. Frames are provided in a canonical order (0..63) so layer and per-layer frame positions can be derived without extra metadata.

## Example
```csharp
// Reconstruct colors for each frame and determine its layer (0..3) and index within the layer (0..15)
for (int frameIndex = 0; frameIndex < gabriel.Frames.Count; frameIndex++)
{
    int layer = frameIndex / 16;          // 0 = DNA, 1 = Traits, 2 = Context, 3 = Live
    int indexInLayer = frameIndex % 16;   // 0..15

    int[] pixelIndices = gabriel.Frames[frameIndex]; // length == 256 for a 16x16 frame
    for (int i = 0; i < pixelIndices.Length; i++)
    {
        int paletteIndex = pixelIndices[i];
        int[] rgb = gabriel.Palette[paletteIndex]; // [r, g, b]
        var color = new RgbColor(rgb[0], rgb[1], rgb[2]);
        // ... render or process color
    }
}
```

## Notes
- Palette entries are expected to be length-3 integer arrays representing [r, g, b] (commonly 0..255); the type does not validate those ranges.
- Frame pixel arrays contain palette indices in the range [0, Palette.Count). Out-of-range indices will cause runtime errors when used to index into Palette.
- Frames are delivered in canonical sequential order (0..63). To determine layer: use floor(frameIndex / 16). To get the position inside a layer: use frameIndex % 16.
- The record's IReadOnlyList wrappers prevent replacing the sequence/collections, but the contained int[] arrays are still mutable; avoid mutating the arrays after construction if immutability is required.
- The Version field indicates the wire-format version; consumers should check it to handle format changes safely.


---

## SequenceMetadataResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

Represents metadata about a generated sequence returned by the API. Use this record when an endpoint or component needs to convey the deterministic seed used to produce a sequence, the timestamp when it was created, and an optional summary of the generator's internal state for debugging or auditing.

## Remarks
This is a compact, positional record intended as an API contract/DTO: Seed enables reproducible recreation of the sequence (when combined with the same generator algorithm and configuration), GeneratedAt records the moment the sequence was produced using a DateTimeOffset to preserve timezone/offset information, and StateSummary is an optional, human-readable snapshot or diagnostic string. As a record, it provides structural equality, a deconstruct method, and with-expressions for easy immutable updates.

## Example
```csharp
// Create a metadata response when a sequence is generated
var meta = new SequenceMetadataResponse(
    Seed: 1234567890L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "position=42;buffered=8");

// Deconstructing
var (seed, generatedAt, summary) = meta;

// Create a copy with an updated summary
var updated = meta with { StateSummary = "position=43;buffered=7" };
```

## Notes
- StateSummary is nullable; absence usually means no diagnostic summary was produced.
- GeneratedAt is a DateTimeOffset — prefer storing/reading it as UTC to avoid ambiguity when comparing timestamps across services.
- The Seed value alone guarantees reproducibility only if the same generator algorithm and configuration are used alongside it.

---
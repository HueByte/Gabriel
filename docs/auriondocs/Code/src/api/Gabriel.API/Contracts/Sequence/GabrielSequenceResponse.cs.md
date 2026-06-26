# GabrielSequenceResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`

## Contents

- [GabrielSequenceResponse](#gabrielsequenceresponse)
- [SequenceMetadataResponse](#sequencemetadataresponse)

---

## GabrielSequenceResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

Represents the wire-format contract for a Gabriel sequence returned by the API. Use this DTO when serializing or deserializing a full sequence payload: it carries a small, palette-indexed representation of every frame (Frames), the palette colors (Palette) as RGB tuples, a protocol Version and additional metadata in Metadata.

## Remarks
The contract keeps frame bodies compact by storing pixels as palette indices rather than full RGB values. Each Palette entry is an [r, g, b] tuple (integers, 0–255). Frames are arrays of palette indices; a typical frame is 16×16 pixels (256 entries) so the Frames list is much smaller on the wire than raw RGB. Frames are provided in canonical order (0..63). Layer identity can be derived from a frame's position: layer = floor(frameIndex / 16) with layers: 0 = DNA, 1 = Traits, 2 = Context, 3 = Live. The Version field lets clients handle backward/forward compatibility; Metadata carries sequence-level info (see SequenceMetadataResponse).

## Example
```csharp
// Reconstruct RGB pixels for the first frame and determine its layer.
IReadOnlyList<int[]> palette = response.Palette;   // each int[] is [r,g,b]
int[] frame0 = response.Frames[0];               // palette indices, expected length 256

// Convert palette index to System.Drawing.Color-like tuple
Color[] paletteColors = palette.Select(p => Color.FromArgb(p[0], p[1], p[2])).ToArray();

// Build a 16x16 color grid for frame 0
Color[,] image = new Color[16, 16];
for (int i = 0; i < 256; i++)
{
    int x = i % 16;
    int y = i / 16;
    int paletteIndex = frame0[i];
    image[x, y] = paletteColors[paletteIndex];
}

// Determine layer for frame 0
int layer = Math.DivRem(0, 16, out _); // or layer = 0 / 16
// layers: 0=DNA, 1=Traits, 2=Context, 3=Live
```

## Notes
- Palette entries are expected to be length-3 integer arrays [r,g,b]; validate before using.
- Frame arrays are expected to contain palette indices (0 <= index < palette.Length); out-of-range indices must be handled as errors.
- Frames are typically 256 elements (16×16) but size is not enforced by the type — validate dimensions if your consumer requires strict sizing.
- Honor the Version field for compatibility checks; do not assume schema or semantics are stable across versions.

---

## SequenceMetadataResponse

> **File:** `src/api/Gabriel.API/Contracts/Sequence/GabrielSequenceResponse.cs`  
> **Kind:** record

Represents metadata returned for a generated sequence: the numeric seed used by the generator, the timestamp when the sequence was produced, and an optional textual summary of the generator's state. Use this record when an API needs to provide lightweight, structured information about how and when a sequence was created.

## Remarks
This is an immutable, positional record intended as an API contract/DTO. Being a record it provides value-based equality, deconstruction, and convenient `with`-expression copying. It is suitable for serialization and for returning from controller actions or service methods that produce sequence information.

## Example
```csharp
// Creating a response to return from a controller
var metadata = new SequenceMetadataResponse(
    Seed: 123456789L,
    GeneratedAt: DateTimeOffset.UtcNow,
    StateSummary: "generator=v2;cursor=42"
);
return Ok(metadata);

// Using with-expression to produce a modified copy
var updated = metadata with { StateSummary = null };
```

## Notes
- StateSummary is nullable; callers should handle a null value when no textual summary is available.
- Prefer using UTC (DateTimeOffset.UtcNow) for GeneratedAt to avoid timezone ambiguities across clients.
- As a positional record, properties are init-only; use the `with` expression to create modified copies rather than mutating in place.

---
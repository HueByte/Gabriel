# Palette

> **File:** `src/api/Gabriel.Engine/Sequence/Palette.cs`  
> **Kind:** record

A fixed, narrow color palette used as the "skin tone" for a Gabriel Sequence. Use Palette when you need a compact, ordered set of RgbColor values that represent a sequence's visual identity (the canonical base color at index 0 and progressively brighter or more saturated variations at higher indices). Palettes are intended to be small coherent sets (typically 8–32 entries) that layers can shift through to produce consistent DNA/Live-State visuals.

## Remarks
Palette is a sealed record that simply wraps an `IReadOnlyList<RgbColor>`, exposing Count and an indexer for convenience. It exists to make a color collection a first-class value in the Sequence model: callers treat a Palette as the canonical source for a sequence's color variants (base color at index 0, higher indices used for brighter/saturated variations). Because Palette is a record it participates in value-like equality and immutability semantics at the object level, while delegating storage to the provided IReadOnlyList.

## Example
```csharp
// Constructing a palette from an existing collection of RgbColor values
var colors = new List<RgbColor> {
    /* RgbColor instances go here */
};
var palette = new Palette(colors);

// Access the canonical base color and a brighter variation
var baseColor = palette[0];
var highlight = palette[Math.Min(2, palette.Count - 1)];
```

## Notes
- Palette[0] is the canonical "base" or quiescent color by convention.
- The code comments recommend palettes be small (typically 8–32 entries) to preserve a coherent visual identity.
- Count forwards to Colors.Count; the indexer is zero-based and will throw if the index is out of range (watch for ArgumentOutOfRangeException).
- Palette wraps an IReadOnlyList but does not guarantee the underlying collection is immutable; if callers need an immutable palette, provide an immutable IReadOnlyList implementation when constructing the Palette.
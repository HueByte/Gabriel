# Palette

> **File:** `src/api/Gabriel.Engine/Sequence/Palette.cs`  
> **Kind:** record

```csharp
public sealed record Palette(IReadOnlyList<RgbColor> Colors)
{
    public int Count => Colors.Count;
    public RgbColor this[int index] => Colors[index];
}
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Colors` | `IReadOnlyList<RgbColor>` | — |


Palette is a fixed, narrow color set designed to feed a Gabriel Sequence's visual identity. It wraps a read-only list of RgbColor values where palette[0] is the canonical base color and higher indices expose brighter or more saturated variations; layers shift across the palette to give DNA / Live State a distinctive signature while keeping the overall appearance coherent.

## Remarks
Palette decouples color data from rendering logic and provides a bounded vocabulary for layer coloring. By exposing Count and an indexer, it enables simple, index-based access to per-layer colors without embedding rendering decisions in callers. The Palette type being a sealed record signals value-like semantics, while still delegating storage to the provided Colors collection.

## Example
```csharp
using System.Collections.Generic;

var baseColor = RgbColor.FromHsv(0.0, 1.0, 1.0);
var variant1 = RgbColor.FromHsv(0.05, 1.0, 0.95);
var variant2 = RgbColor.FromHsv(0.1, 0.9, 0.9);

var palette = new Palette(new List<RgbColor> { baseColor, variant1, variant2 });

RgbColor baseEntry = palette[0];
int count = palette.Count;
```

## Notes
- Accessing an index outside the available range (0..Count-1) will throw an exception from the underlying Colors collection.
- Palette does not defensively copy the provided Colors collection; if that collection is mutable and modified after construction, callers may observe those changes. If true immutability is required, pass an immutable collection or clone the data before constructing the Palette.
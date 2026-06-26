# Palette

> **File:** `src/api/Gabriel.Engine/Sequence/Palette.cs`  
> **Kind:** record

Represents a small, ordered color palette (a "skin tone") used by the Gabriel Sequence. Reach for this when you need a compact, indexable collection of RgbColor values where index 0 is the canonical base/quiescent color and higher indices provide progressively brighter or more saturated variants for layer-level differentiation.

## Remarks
This sealed record wraps an `IReadOnlyList<RgbColor>` to provide a lightweight, value-like container for a fixed narrow palette (typically 8–32 entries in the system's style). It exists to give sequences and layers a consistent visual identity: consumers index into the palette to select a base color or a related variant rather than constructing ad-hoc colors. The record itself does not enforce palette length or immutability of the underlying collection — it only exposes the provided list and convenience accessors.

## Example
```csharp
// Create a palette (array implicitly implements IReadOnlyList<T>)
var palette = new Palette(new[]
{
    new RgbColor(0x10, 0x20, 0x30), // index 0: base/quiescent
    new RgbColor(0x22, 0x33, 0x44), // index 1: brighter/saturated variant
    new RgbColor(0x33, 0x55, 0x66), // etc.
});

RgbColor baseColor = palette[0];
int entries = palette.Count;

// Use palette values when rendering or assigning layer colors
// layer.Color = palette[layerPaletteIndex];
```

## Notes
- The Palette stores the provided `IReadOnlyList<RgbColor>` reference; if you pass a mutable `List<T>` the contents can still change. If immutability is required, pass an immutable collection or a defensive copy.
- There is no validation of palette length; callers should ensure the list has the expected number of entries (the system commonly uses 8–32 entries for a coherent visual identity).
- Equality for the record is based on the Colors property value — for reference types that typically means the collection reference is compared, not the sequence contents. Two different lists with identical color sequences will not be equal unless they are the same object or you compare sequences explicitly.
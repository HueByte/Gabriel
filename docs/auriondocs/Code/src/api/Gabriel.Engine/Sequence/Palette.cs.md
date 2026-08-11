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


Palette represents a fixed, narrow color set used by the Gabriel Sequence to convey a coherent visual identity. It wraps an `IReadOnlyList<RgbColor>` (Colors) and exposes simple, index-based access along with a Count, enabling consumers to treat the palette as a compact collection of base and variation colors. Palette[0] is the canonical base color, and higher indices are brighter or more saturated variations; layers can shift toward different regions of the palette to give DNA / Live State a distinct visual signature while keeping the palette size within the spec (typically 8–32 entries).

## Remarks
Palette centralizes color management for sequences that require a repeatable, descriptive color identity. By encapsulating Colors behind a dedicated Palette type, you decouple the rendering logic from concrete color values and make it easier to switch or reuse color sets across different sequences. This abstraction also clarifies intent: index 0 is the base, while subsequent positions encode progressive variations used to distinguish layers or states.

## Notes
- The Colors collection is exposed as IReadOnlyList; ensure the underlying list is not mutated after Palette creation if you need true immutability, or wrap the source in an immutable read-only collection.

## Dependencies
- RgbColor
- Sequence
- Colors

## Dependency APIs (verified signatures)
- record [`RgbColor`](RgbColor.cs.md) (`src/api/Gabriel.Engine/Sequence/RgbColor.cs`)
  - `RgbColor FromHsv(double hue, double saturation, double value)`

## Symbol To Document
- Name: `Palette`
- Kind: `record`
- File: `src/api/Gabriel.Engine/Sequence/Palette.cs`
- Language: `csharp`
- ID: `023d31c1-fd23-49a5-aff7-11c5e148b1da`
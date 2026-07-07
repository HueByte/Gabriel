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


Palette represents a compact, read-only palette of colors used by a Gabriel Sequence to encode per-layer variation while preserving a coherent visual identity. It wraps a single read-only list of RgbColor values and exposes convenient access via a Count property and an indexer, so callers can sample colors without mutating the underlying collection. Palette[0] is the canonical base color; higher indices are brighter or more saturated variations. As layers render, they shift toward different regions of the palette to give the sequence's DNA or Live State a distinctive visual signature.

## Remarks
By encapsulating the color data behind Palette, the rendering code can rely on a small, stable palette contract across sequences. It isolates color semantics from layout or logic, enabling easy replacement of the color set to achieve new aesthetics without touching rendering code. Palette acts as a contract: consumers access colors by index and can rely on a fixed base at index 0 to anchor the visual identity.

## Notes
- Colors must be non-null; the constructor does not guard against null values.
- The expected palette size in the domain is typically 8–32 entries; this constraint is not enforced at compile time.
- Palette equality is determined by the Colors property; two Palettes with identical contents in separate list instances may not compare equal, since equality includes the Colors reference.

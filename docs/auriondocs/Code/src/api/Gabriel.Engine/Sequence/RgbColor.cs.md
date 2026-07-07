# RgbColor

> **File:** `src/api/Gabriel.Engine/Sequence/RgbColor.cs`  
> **Kind:** record

```csharp
public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static RgbColor FromHsv(double hue, double saturation, double value)
    {

        var h = (hue % 1.0 + 1.0) % 1.0 * 6.0;
        var c = value * saturation;
        var x = c * (1 - Math.Abs(h % 2 - 1));
        var m = value - c;
        var (r, g, b) = (int)Math.Floor(h) switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return new RgbColor(
            (byte)Math.Clamp((r + m) * 255, 0, 255),
            (byte)Math.Clamp((g + m) * 255, 0, 255),
            (byte)Math.Clamp((b + m) * 255, 0, 255));
    }
}
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `R` | `byte` | — |
| `G` | `byte` | — |
| `B` | `byte` | — |


RgbColor is a compact, immutable RGB color value type with 8-bit channels designed to be embedded directly in color palettes without heap pressure. It exposes a helper FromHsv to convert standard HSV color components into an RGB color efficiently.

## Remarks
As a readonly record struct, it offers value-based equality with cheap copying while remaining a small, stack-friendly value type. The FromHsv method implements a standard HSV-to-RGB conversion and returns a new color with clamped 0–255 channels, keeping the color representation simple and predictable for palette generation and rendering pipelines.

## Example
```csharp
var red = RgbColor.FromHsv(0.0, 1.0, 1.0); // pure red
```

## Notes
- Hue values outside [0, 1) are normalized internally (wrapped) before conversion.
- The resulting RgbColor channels are clamped to the 0–255 range, ensuring valid byte components even for extreme HSV inputs.
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


RgbColor is a compact RGB color value using 8-bit channels, implemented as a readonly record struct for use as a value type. It is designed to be inlined in arrays of palette entries to avoid heap allocations. It provides FromHsv(double hue, double saturation, double value) which performs the standard HSV→RGB conversion (hue in [0,1), saturation and value in [0,1]), then scales and clamps the result to 0–255 per channel to produce a new RgbColor.

## Remarks
Its readonly record struct nature makes it immutable and cheap to copy, ideal for embedding directly in color palettes.
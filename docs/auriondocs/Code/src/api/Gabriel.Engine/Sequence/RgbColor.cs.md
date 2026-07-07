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


RgbColor is a compact, immutable RGB color token stored as a three-byte triplet inside a value-type struct, ideal for palette entries and large inline color arrays where heap allocations must be avoided. When you need to derive a color from HSV values, FromHsv provides a standard HSV-to-RGB conversion and returns an RgbColor with 8-bit channels.

## Remarks
Because it is a readonly record struct, it enjoys value semantics and cheap copying while remaining allocation-free. The color channels are exposed as R, G, and B bytes, making it straightforward to store in color palettes and pass around as a small token. The FromHsv method uses a conventional HSV-to-RGB mapping with hue in [0,1), and saturation/value in [0,1], clamping each resulting channel to the 0–255 range.

## Example
```csharp
var red = RgbColor.FromHsv(0.0, 1.0, 1.0);
Console.WriteLine($"{red.R},{red.G},{red.B}"); // 255,0,0
```

## Notes
- Hue is normalized to the [0,1) interval inside the conversion (hue % 1.0 + 1.0) % 1.0), while saturation and value are treated within [0,1].
- Each channel is clamped to the 0–255 range after the HSV→RGB computation, so inputs that would overflow simply saturate at the extremes.
- Saturation or value of 0 yields grayscale results; the mapping follows the standard HSV→RGB conversion semantics.}
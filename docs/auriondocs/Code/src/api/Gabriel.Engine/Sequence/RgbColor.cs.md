# RgbColor

> **File:** `src/api/Gabriel.Engine/Sequence/RgbColor.cs`  
> **Kind:** record

A compact, immutable RGB color stored as three 8-bit channels (R, G, B). Use this value type when you need lightweight color values that can be stored in arrays or palettes without heap allocation and with value-based equality semantics.

## Remarks
This is a readonly record struct so instances are immutable and cheap to store in large arrays or other value-type collections; the type was chosen to avoid heap pressure when palette entries are stored inline. Channel values are bytes in the 0–255 range. The static FromHsv method provides a standard HSV→RGB conversion: hue is treated as a fraction of a full circle, saturation and value are expected in [0,1], and the result is scaled to 8-bit channels.

## Example
```csharp
// Construct directly from bytes
var orange = new RgbColor(255, 165, 0);

// Create from HSV (hue in [0,1), saturation and value in [0,1])
// hue = 0.08 (≈ 29°), saturation = 1, value = 1 -> vivid orange
var fromHsv = RgbColor.FromHsv(0.08, 1.0, 1.0);
```

## Notes
- Hue is wrapped using modulo arithmetic, so values outside [0,1) are normalized into that range before conversion.
- Saturation and value are not clamped before internal computation; the final byte channels are clamped to [0,255], so out-of-range inputs will be coerced into valid byte values.
- As a readonly record struct, the type is immutable and uses value semantics for equality and copying; copying small structs is cheap but be mindful when used in very large collections or performance-critical paths.
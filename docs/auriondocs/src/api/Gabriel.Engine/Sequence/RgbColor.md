# RgbColor

> **File:** `src/api/Gabriel.Engine/Sequence/RgbColor.cs`  
> **Kind:** record

Represents a single RGB triplet with 8 bits per channel as an immutable value type. Designed for compact in-memory palettes and other scenarios where avoiding heap allocations matters; use RgbColor.FromHsv to convert normalized HSV components into this 8-bit-per-channel representation.

## Remarks
This is a readonly record struct so instances are value types (stack-allocated or inlined in arrays) and immutable, reducing GC pressure when storing many colors. The FromHsv method implements a standard HSV→RGB conversion: hue is treated cyclically, saturation and value are expected in the [0,1] range, and the resulting channels are clamped and quantized to bytes.

## Example
```csharp
// Create an RgbColor directly
var red = new RgbColor(255, 0, 0);

// Create from HSV: hue in [0,1) (0 = red), saturation and value in [0,1]
var pastelBlue = RgbColor.FromHsv(0.6, 0.4, 0.9);

// Deconstructing
var (r, g, b) = pastelBlue;
Console.WriteLine($"R: {r}, G: {g}, B: {b}");
```

## Notes
- Hue is wrapped using modulo arithmetic, so values outside [0,1] are accepted and rotated into range.
- Inputs are treated as normalized (saturation and value in 0..1); the method does not throw for out-of-range doubles but output is clamped to byte range.
- Colors are quantized to 8 bits per channel; this causes rounding compared with higher-precision representations.
- The type's immutability and value semantics make it thread-safe for sharing without synchronization.
# Patterns

> **File:** `src/api/Gabriel.Engine/Sequence/Patterns.cs`  
> **Kind:** class

```csharp
internal static class Patterns
```


A collection of small, deterministic 2D procedural pattern primitives that map a pixel coordinate (x,y) and a normalized time t ∈ [0,1) to a scalar in [0,1]. Use these when you need loop-continuous animated pattern samples for palette indexing; each pattern exposes an Init helper that produces a randomized parameter record and a sampling function that accepts (x, y, t, params).

## Remarks
These patterns are lightweight building blocks intended for a frame generator that converts scalar values to palette indices. They are designed for loop continuity (sampling at t = 0 and t = 1 should produce the same image) and return values normalized to [0,1]. Each pattern pairs a readonly record struct (parameters) with an Init method that seeds reasonable defaults from a Random plus a sampling function that takes the parameter record by in to avoid copies. Parameters commonly control frequency, phase/speed, sharpness (via Math.Pow), and a pattern center (Cx, Cy) which is initialized to size / 2.0 - 0.5 so coordinates are pixel-centered.

## Example
```csharp
// Create a deterministic Plasma pattern for a given canvas size
var rng = new Random(12345);
int size = 64; // canvas size used to compute Cx/Cy
var plasmaParams = Patterns.PlasmaInit(rng, size);

// sample at pixel (x, y) and normalized time t (0 <= t < 1)
double x = 10.5, y = 20.5, t = 0.25;
double value = Patterns.Plasma(x, y, t, in plasmaParams);
// 'value' is in [0, 1] and can be mapped to a palette index.

// Waves usage is analogous:
var wavesParams = Patterns.WavesInit(rng, size);
double w = Patterns.Waves(x, y, t, in wavesParams);
```

## Notes
- t is expected to be normalized to the cycle [0,1). Although the functions mathematically handle t values outside that range, the class is authored with loop continuity in mind (t==0 and t==1 should match).
- Sharpness parameters are applied as exponents (Math.Pow) and increase contrast; values significantly >1 make transitions steeper.
- Parameter records include Cx/Cy set to size/2 - 0.5 to center patterns on pixel grids; keep that convention when supplying custom params.

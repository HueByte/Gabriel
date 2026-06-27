# Patterns

> **File:** `src/api/Gabriel.Engine/Sequence/Patterns.cs`  
> **Kind:** class

Procedural pixel-space pattern primitives used by the sequence generator. Each pattern function samples a scalar in [0, 1] for a given coordinate (x, y) and a normalized time t (0 ≤ t < 1); callers typically use the returned value to select a color from a palette. Use the corresponding *Init methods to produce sensible, size-aware parameter structs, or construct parameter records manually when you need repeatable or specific behaviour.

## Remarks
These patterns are designed for temporal loop continuity — sampling at t = 0 and t = 1 should produce the same frame so multi-frame cycles close seamlessly. The class intentionally separates pattern generation (returns a normalized scalar) from palette mapping so the same pattern can be reused with different color schemes or post-processing. All pattern functions are pure and deterministic given their inputs; the Init helpers encapsulate randomized parameter selection tuned to the typical canvas size.

## Example
```csharp
// Typical usage: initialize parameters for a canvas size, then sample per-pixel
var rng = new Random(42);
int size = 64; // canvas/resolution size used when creating center offsets
var p = Patterns.PlasmaInit(rng, size);
double t = frameIndex / 64.0; // normalized time in [0,1)
for (int y = 0; y < size; y++)
{
    for (int x = 0; x < size; x++)
    {
        double v = Patterns.Plasma(x, y, t, in p); // v is in [0,1]
        int colorIndex = (int)Math.Round(v * (palette.Length - 1));
        // use colorIndex to look up a color from a palette
    }
}
```

## Notes
- t is normalized to the unit interval (0 ≤ t < 1). Treat t = 1 as equivalent to t = 0 to preserve loop continuity.
- Outputs are normalized scalars in [0, 1]; callers map this to palette indices or other encodings.
- The Init helpers set Cx/Cy to size / 2 - 0.5; if you construct parameter records manually, ensure the center coordinates match your intended sampling geometry.
- Pattern functions perform trigonometric and other math ops (Sin, Sqrt, Atan2). Sampling densely (per-pixel each frame) can be CPU-intensive; consider precomputing, caching, or downgrading sample rate for performance-sensitive scenarios.
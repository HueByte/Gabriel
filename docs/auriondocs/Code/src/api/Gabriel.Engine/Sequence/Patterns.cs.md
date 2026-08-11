# Patterns

> **File:** `src/api/Gabriel.Engine/Sequence/Patterns.cs`  
> **Kind:** class

```csharp
internal static class Patterns
```


A collection of small, deterministic spatial+temporal pattern primitives used by the generator to produce looping animated values. Each pattern is a pure function that maps a coordinate (x, y) and a normalized time t (expected in [0, 1)) plus a pattern-specific parameter record to a scalar in [0, 1]. Use these primitives when you want reusable, compact shape/texture sources that the renderer will convert to palette indices; prefer them over embedding raw math in rendering code because the class centralizes loop-continuous, parameterized patterns and provides Init helpers to produce varied but well-formed parameter sets.

## Remarks
The class separates pattern generation (spatial + temporal math) from palette mapping and frame construction. Each pattern offers an Init method (taking a Random and the canvas size) that produces a small readonly record of parameters; the corresponding pattern function is a stateless evaluator that accepts those parameters by reference (`in`). This design keeps the evaluators cheap and thread-friendly (pure math) while allowing randomized, size-aware parameter creation. Patterns are intentionally continuous between t=0 and t=1 so multiple primitives can be cycled or cross-faded without visible seams.

## Example
```csharp
// create parameters for a 16x16 pattern and sample one pixel over a 64-frame cycle
var rng = new Random(12345);
int size = 16;
var plasmaParams = Patterns.PlasmaInit(rng, size);
for (int frame = 0; frame < 64; frame++)
{
    double t = frame / 64.0; // normalized time in [0,1)
    double value = Patterns.Plasma(x: 7.5, y: 7.5, t: t, p: plasmaParams);
    // 'value' is in [0,1]; map to palette index outside this helper
}
```

## Notes
- t is normalized: treat t==1 as equivalent to t==0 for seamless looping; callers should provide t in [0,1).
- The Init helpers use the supplied Random and the canvas size to set sensible defaults (for example Cx/Cy = size/2 - 0.5). If you need a different coordinate origin, construct the params manually.
- Many patterns expose a Sharpness/exponent parameter; values >1 concentrate outputs toward 0 or 1 and may reduce visible mid-range palette variation.
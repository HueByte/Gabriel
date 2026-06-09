# Patterns

> **File:** `src/api/Gabriel.Engine/Sequence/Patterns.cs`  
> **Kind:** class

Procedural pattern primitives used to produce loop-continuous animated 2D scalar fields. Each pattern function maps a coordinate (x, y) and a normalized time t in [0, 1) plus a pattern-specific parameter record to a scalar in [0, 1]; the generator maps that scalar to a palette index. Use the provided Init helpers (e.g. PlasmaInit, WavesInit, SpiralInit) to create randomized, size-aware parameter records suitable for sampling across a pixel grid.

## Remarks
Patterns centralizes several compact, reusable procedural generators (Plasma, Waves, Spiral, Pulse, Shimmer, etc.). Each pattern is implemented as a pure function that depends only on its inputs and an immutable parameter record; the Init helpers encapsulate randomized parameter selection and compute sensible defaults such as the pattern center (Cx, Cy) from the image size. The functions are designed for temporal loop continuity: sampling at t = 0 and t = 1 yields the same result so frame cycles close seamlessly.

## Example
```csharp
// Create deterministic parameters and sample a pattern for a pixel
var rng = new Random(seed: 12345);
int size = 64; // grid size used to center patterns
var p = Patterns.PlasmaInit(rng, size);
// sample at pixel (x, y) and normalized time t in [0,1)
double x = 10.0, y = 22.0, t = 0.25;
double scalar = Patterns.Plasma(x, y, t, in p);
// Map to palette index (example)
int paletteIndex = (int)(scalar * (palette.Length - 1));
```

## Notes
- t must be normalized to the half-open interval [0, 1) (or wrapped) to preserve the intended loop continuity; passing values outside this without wrapping breaks the loop assumption. 
- The Init helpers set Cx/Cy to size/2.0 - 0.5 so patterns are centered on pixel grids; if you compute your own parameters make sure to use the same coordinate convention. 
- Returned values are in [0, 1]; many patterns apply a power/`Sharpness` exponent which concentrates values toward 0 or 1 — small changes in Sharpness can noticeably change contrast.
- All pattern methods are pure and thread-safe (they do not mutate shared state); parameter records are immutable record structs and are passed by reference with `in` for efficiency.
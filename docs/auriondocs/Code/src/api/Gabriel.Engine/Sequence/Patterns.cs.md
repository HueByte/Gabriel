# Patterns

> **File:** `src/api/Gabriel.Engine/Sequence/Patterns.cs`  
> **Kind:** class

```csharp
internal static class Patterns
```


A collection of small, deterministic procedural pattern generators used to produce scalar values in the range [0, 1] from a pixel coordinate (x, y) and a normalized time t. Each pattern exposes a Params record type, an Init(Random, size) helper that seeds pattern parameters (typically using the provided RNG and image size), and a sampling function that evaluates the pattern at (x, y, t). These patterns are intended for palette-based generators and are designed for loop continuity so sampling at t = 0 and t = 1 yields the same frame.

## Remarks
The class centralizes multiple visual primitives (plasma, directional waves, spirals, pulses, etc.) and separates parameter initialization from sampling. This makes it straightforward to produce repeatable visuals by seeding the provided Random and reusing the returned Params records across many sample calls. Init methods compute size-aware centers (Cx, Cy) as size / 2.0 - 0.5 so patterns are naturally centered for a square grid. The sampling functions use trigonometric combinations and exponentiation (sharpness) to produce varied and tunable textures while guaranteeing temporal wraparound for smooth looping.

## Example
```csharp
// Create a deterministic plasma pattern for a 64x64 grid and sample one frame.
var rng = new Random(12345);
int size = 64;
var plasmaParams = Patterns.PlasmaInit(rng, size);
double t = 0.25; // normalized time in [0,1)
int paletteLength = 16;
var frame = new int[size][];
for (int y = 0; y < size; y++)
{
    frame[y] = new int[size];
    for (int x = 0; x < size; x++)
    {
        double v = Patterns.Plasma(x, y, t, in plasmaParams); // v in [0,1]
        int index = (int)Math.Floor(v * paletteLength) % paletteLength;
        frame[y][x] = index;
    }
}
// `frame` now contains palette indices for the sampled plasma pattern.
```

## Notes
- t is a normalized time value on [0,1). The functions use TwoPi * t, so t values differing by integer amounts produce identical results (t = 1 wraps to t = 0).
- Init methods set Cx and Cy to size / 2.0 - 0.5; pass x/y coordinates that match this pixel-center convention (0..size-1) for correct centering.
- The Init methods use the caller-supplied Random; for reproducible patterns, reuse a seeded Random. Sampling functions themselves are pure and thread-safe, but Init is not thread-safe if the same Random is used concurrently.
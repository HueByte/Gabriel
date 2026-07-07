# Noise

> **File:** `src/api/Gabriel.Engine/Sequence/Noise.cs`  
> **Kind:** class

```csharp
internal static class Noise
```


Deterministic noise primitives powering the Gabriel Sequence generator. This internal static class consolidates hash-based value noise and fractal Brownian motion to produce repeatable, visually coherent patterns, matching the hash/value/fbm shape used in the webapp's pulse/patterns.ts to keep the visual language consistent across sequences and procedural avatars.

## Remarks
This class provides a compact, reusable noise toolkit for deterministic visuals. Hash2 maps (x, y, seed) to a uniform double in [0, 1), serving as the corner-values for bilinear interpolation in Value. The private Smooth function applies cubic Hermite smoothing to produce smooth transitions. Value samples four hashed corners and blends them with bilinear interpolation, yielding smooth value noise. Fbm layers multiple octaves of Value at doubling frequencies and halving amplitudes, normalizing the result to roughly [0, 1] to produce natural-looking fractal noise suitable for textures and procedural elements. All members are static, enabling convenient reuse without instance state, and the implementation mirrors common value-noise and fbm patterns to ensure visuals align with the broader system.

## Example
```csharp
// Example usage: obtain a smooth fractal noise value for a point
double n = Noise.Fbm(0.42, 1.17, 1234, 6);
```

## Notes
- Hash2 uses unchecked integer arithmetic to produce a deterministic 32-bit hash; the overflow is intentional and reproducible. The final value is cast to a double in [0, 1].
- Fbm returns a value in roughly [0, 1], but exact bounds depend on the number of octaves and sampling, so callers should treat it as a stable noise range rather than a strict bound.
- The class is internal to the assembly; it is intended for use within Gabriel Engine components (sequences, avatars, and related visuals) rather than as a public API.
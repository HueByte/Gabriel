# Noise

> **File:** `src/api/Gabriel.Engine/Sequence/Noise.cs`  
> **Kind:** class

```csharp
internal static class Noise
```


Deterministic noise primitives for the Gabriel Sequence generator; it provides repeatable, coordinate-based values that mirror the webapp’s pulse and patterns noise grammar. Developers reach for Noise when they need stable pseudo-random input tied to (x, y) coordinates and a seed, rather than true randomness.

## Remarks
Centralizes noise-generation concerns to keep the engine and UI in sync on how noise looks. The implementation uses an unchecked 32-bit mix to produce a deterministic output in [0,1) from integer inputs, ensuring cross-platform reproducibility. Value performs bilinear interpolation across the four lattice points with a cubic Hermite smoothstep to yield smooth transitions. Fbm stacks octaves of Value with halving amplitude and doubling frequency to produce a stable fractal noise that remains roughly in [0,1].

## Notes
- Not cryptographically secure; use for visuals, not security-sensitive tasks.
- Internal API; not exposed publicly.
- Fbm can be computationally intensive; adjust octaves to balance quality and performance.
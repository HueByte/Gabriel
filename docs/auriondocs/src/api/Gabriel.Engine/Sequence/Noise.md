# Noise

> **File:** `src/api/Gabriel.Engine/Sequence/Noise.cs`  
> **Kind:** class

Deterministic 2D noise primitives used by the Gabriel Sequence generator. Use these helpers when you need repeatable, platform-independent value noise and fractal noise (fBm) that match the webapp's pulse/patterns.ts noise grammar so visuals remain consistent across components.

## Remarks
This class provides a small, self-contained noise grammar: Hash2 produces a reproducible integer-based pseudorandom value per lattice point; Value performs bilinear interpolation between lattice hashes using a cubic Hermite ease (smoothstep); Fbm sums multiple octaves of Value at doubling frequencies and halving amplitudes and normalizes the result. The implementation intentionally mirrors the web application's noise shape so procedural avatars and sequence visuals share the same statistical behaviour.

## Example
```csharp
// Sample fractional Brownian motion at scaled coordinates with 4 octaves
double x = 120.0, y = 80.0;
int seed = 7;
double scale = 0.01; // scale controls feature size
double noiseValue = Noise.Fbm(x * scale, y * scale, seed, 4);
// noiseValue is approximately in [0, 1]
```

## Notes
- Hash2 returns a deterministic pseudo-random value derived from integer inputs; due to the implementation detail (division by 4294967295.0) the result can be exactly 1.0 in rare cases — treat the range as approximately [0, 1].
- Value uses Math.Floor to select lattice points and bilinear interpolation with a smoothstep easing; sampling at integer coordinates returns the hashed lattice values directly.
- Fbm offsets the seed per octave using `seed + i * 17`, doubles frequency each octave and halves amplitude, and normalizes by the sum of amplitudes so output remains roughly in the same range regardless of octave count.
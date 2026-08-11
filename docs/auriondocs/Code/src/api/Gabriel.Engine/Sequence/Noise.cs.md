# Noise

> **File:** `src/api/Gabriel.Engine/Sequence/Noise.cs`  
> **Kind:** class

```csharp
internal static class Noise
```


Deterministic 2D noise primitives for the Gabriel Sequence generator, yielding repeatable variation that matches the web app's pulse/patterns noise language. It exposes a hashed 2D sampler (Hash2), a smooth value-noise interpolator (Value), and a multi-octave fractal noise builder (Fbm) for richer textures across scales.

## Remarks
Hash2 is a non-cryptographic hash that maps integer coordinates and a seed to a deterministic pseudo-random value. Value performs bilinear interpolation of the surrounding lattice values with a cubic smoothstep to produce smooth noise, while Fbm stacks octaves of Value with doubling frequency and halving amplitude, normalizing the result to roughly [0,1]. This separation of concerns makes it easy to swap noise strategies or reuse these primitives in other visual generators without changing callers.

## Notes
- Hash2's output is deterministic but not cryptographically secure; use it for noise, not security.
- Hash2 may produce 1.0 in rare cases due to normalization; clamp if you need strictly less than 1.0.
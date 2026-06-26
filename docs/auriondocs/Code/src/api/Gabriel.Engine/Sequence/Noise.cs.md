# Noise

> **File:** `src/api/Gabriel.Engine/Sequence/Noise.cs`  
> **Kind:** class

```csharp
// Deterministic noise primitives for the Gabriel Sequence generator. Mirrors
// the hash + value-noise + fbm shape that lives in the webapp's pulse/patterns.ts
// so the visual language stays consistent - Sequences and the existing
// procedural avatar speak the same noise grammar.
internal static class Noise
{
    // Integer hash of (x, y, seed) → uniform double in [0, 1).
    public static double Hash2(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393) + (uint)(y * 668265263) + (uint)(seed * 982451653);
            h = (h ^ (h >> 13)) * 1274126177;
            return ((h ^ (h >> 16)) & 0xFFFFFFFFu) / 4294967295.0;
        }
    }

    // Smoothstep - classic cubic Hermite ease.
    private static double Smooth(double t) => t * t * (3 - 2 * t);

    // Value noise sampled bilinearly between integer hash points.
    public static double Value(double x, double y, int seed)
    {
        var xi = (int)Math.Floor(x);
        var yi = (int)Math.Floor(y);
        var xf = x - xi;
        var yf = y - yi;
        var a = Hash2(xi,     yi,     seed);
        var b = Hash2(xi + 1, yi,     seed);
        var c = Hash2(xi,     yi + 1, seed);
        var d = Hash2(xi + 1, yi + 1, seed);
        var u = Smooth(xf);
        var v = Smooth(yf);
        return a * (1 - u) * (1 - v)
             + b * u       * (1 - v)
             + c * (1 - u) * v
             + d * u       * v;
    }

    // Fractional Brownian motion - sums octaves of value noise at doubling
    // frequencies + halving amplitudes. Output is normalized to roughly [0, 1].
    public static double Fbm(double x, double y, int seed, int octaves)
    {
        double sum = 0, amp = 1, total = 0, freq = 1;
        for (var i = 0; i < octaves; i++)
        {
            sum += Value(x * freq, y * freq, seed + i * 17) * amp;
            total += amp;
            amp *= 0.5;
            freq *= 2;
        }
        return sum / total;
    }
}
```


Deterministic, pure noise primitives used by the Gabriel Sequence generator. Use these when you need the same hash + value-noise + fractional Brownian motion (fbm) behavior the webapp uses so procedural visuals produced by the engine remain visually consistent with the web application's pulse/patterns.ts implementation.

## Remarks
This class mirrors the noise grammar used by the web client: a small integer hash (Hash2), a bilinearly-interpolated value noise (Value) with a cubic Hermite ease, and an fbm aggregator (Fbm) that stacks octaves at doubling frequency and halving amplitude. All methods are pure and deterministic (they rely on unchecked uint arithmetic and integer wrap-around), making them safe to call from multiple threads and ideal for reproducible procedural content.

## Example
```csharp
// Generate a normalized fbm value and convert to an 8-bit grayscale sample
double x = 12.34, y = 56.78;
int seed = 42;
int octaves = 4;

double n = Noise.Fbm(x, y, seed, octaves); // roughly in [0, 1]
byte gray = (byte)(Math.Max(0.0, Math.Min(1.0, n)) * 255.0);

// gray can be used as a pixel intensity or further remapped
```

## Notes
- Hash2 relies on unchecked integer overflow and bitwise operations for speed and determinism; this is intentional but worth noting if porting to languages/platforms with different overflow semantics.
- The Hash2 comment claims output in [0, 1) but the implementation divides by 4294967295.0 (uint max). If the hashed uint equals 0xFFFFFFFF the result would be exactly 1.0; use caution if strict half-open range is required.
- Fbm offsets the seed per-octave by adding i * 17 to avoid octave correlation; octaves linearly increase CPU cost (more calls to Value) but are normalized by the total amplitude at the end.
- All methods are deterministic and side-effect free; they can be freely called from multiple threads without synchronization.
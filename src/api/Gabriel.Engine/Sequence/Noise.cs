namespace Gabriel.Engine.Sequence;

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

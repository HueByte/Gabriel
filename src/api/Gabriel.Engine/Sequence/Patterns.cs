namespace Gabriel.Engine.Sequence;

// Pattern primitives ported (with tweaks) from the webapp's pulse/patterns.ts.
// Each pattern takes a coordinate (x, y) and a normalized time t in [0, 1) +
// pattern-specific parameters; returns a scalar in [0, 1] that the generator
// maps to a palette index.
//
// Patterns are designed for LOOP CONTINUITY — sampling at t=0 and t=1 should
// produce the same image so the 64-frame cycle closes seamlessly.
internal static class Patterns
{
    private const double TwoPi = Math.PI * 2;

    // --- Plasma ----------------------------------------------------------------
    //
    // Superposed sines in x, y, x+y, and radial-distance. Gorgeous out-of-the-box
    // animation, hugely varied across the parameter space — small seed changes
    // produce noticeably different visuals.

    public readonly record struct PlasmaParams(
        double A, double B, double C, double D,
        double Sa, double Sb, double Sc, double Sd,
        double Cx, double Cy);

    public static PlasmaParams PlasmaInit(Random rng, int size)
    {
        return new PlasmaParams(
            A: Range(rng, 0.35, 0.70),
            B: Range(rng, 0.35, 0.70),
            C: Range(rng, 0.25, 0.60),
            D: Range(rng, 0.40, 0.80),
            Sa: Pick(rng, new[] { 1.0, -1.0, 2.0, -2.0 }),
            Sb: Pick(rng, new[] { 1.0, -1.0, 2.0 }),
            Sc: Pick(rng, new[] { 1.0, -1.0 }),
            Sd: Pick(rng, new[] { 1.0, 2.0 }),
            Cx: size / 2.0 - 0.5,
            Cy: size / 2.0 - 0.5);
    }

    public static double Plasma(double x, double y, double t, in PlasmaParams p)
    {
        var phase = TwoPi * t;
        var v = (
            Math.Sin(x * p.A + phase * p.Sa) +
            Math.Sin(y * p.B + phase * p.Sb) +
            Math.Sin((x + y) * p.C + phase * p.Sc) +
            Math.Sin(Math.Sqrt((x - p.Cx) * (x - p.Cx) + (y - p.Cy) * (y - p.Cy)) * p.D + phase * p.Sd)
        ) / 4.0;
        return (v + 1) / 2;
    }

    // --- Directional waves -----------------------------------------------------

    public readonly record struct WavesParams(
        double Angle, double Freq, double Speed, double Sharpness, double Cx, double Cy);

    public static WavesParams WavesInit(Random rng, int size)
    {
        return new WavesParams(
            Angle: Range(rng, 0, TwoPi),
            Freq: Range(rng, 0.6, 1.4),
            Speed: rng.NextDouble() < 0.5 ? 1.0 : -1.0,
            Sharpness: Range(rng, 1.2, 2.4),
            Cx: size / 2.0 - 0.5,
            Cy: size / 2.0 - 0.5);
    }

    public static double Waves(double x, double y, double t, in WavesParams p)
    {
        var proj = (x - p.Cx) * Math.Cos(p.Angle) + (y - p.Cy) * Math.Sin(p.Angle);
        var s = Math.Sin(proj * p.Freq + p.Speed * TwoPi * t);
        return Math.Pow((s + 1) / 2, p.Sharpness);
    }

    // --- Spiral ----------------------------------------------------------------

    public readonly record struct SpiralParams(
        int Arms, double Tightness, double Speed, double Sharpness, double Cx, double Cy);

    public static SpiralParams SpiralInit(Random rng, int size)
    {
        return new SpiralParams(
            Arms: Pick(rng, new[] { 1, 2, 3 }),
            Tightness: Range(rng, 0.5, 1.1),
            Speed: rng.NextDouble() < 0.5 ? 1.0 : -1.0,
            Sharpness: Range(rng, 1.4, 2.4),
            Cx: size / 2.0 - 0.5,
            Cy: size / 2.0 - 0.5);
    }

    public static double Spiral(double x, double y, double t, in SpiralParams p)
    {
        var dx = x - p.Cx;
        var dy = y - p.Cy;
        var theta = Math.Atan2(dy, dx);
        var dist = Math.Sqrt(dx * dx + dy * dy);
        var s = Math.Sin(theta * p.Arms + dist * p.Tightness - p.Speed * TwoPi * t);
        return Math.Pow((s + 1) / 2, p.Sharpness);
    }

    // --- Pulse (expanding ripples) --------------------------------------------

    public readonly record struct PulseParams(
        double Cx, double Cy, double WaveWidth, int Ripples, double MaxRadius);

    public static PulseParams PulseInit(Random rng, int size)
    {
        var waveWidth = Range(rng, 1.8, 3.2);
        return new PulseParams(
            Cx: Range(rng, size / 2.0 - 1, size / 2.0 + 1),
            Cy: Range(rng, size / 2.0 - 1, size / 2.0 + 1),
            WaveWidth: waveWidth,
            Ripples: Pick(rng, new[] { 1, 2 }),
            MaxRadius: Math.Sqrt(size * size + size * size) / 2 + waveWidth);
    }

    public static double Pulse(double x, double y, double t, in PulseParams p)
    {
        var dist = Math.Sqrt((x - p.Cx + 0.5) * (x - p.Cx + 0.5) + (y - p.Cy + 0.5) * (y - p.Cy + 0.5));
        var v = 0.0;
        for (var r = 0; r < p.Ripples; r++)
        {
            var rPhase = (t + (double)r / p.Ripples) % 1.0;
            var waveR = rPhase * p.MaxRadius;
            var delta = Math.Abs(dist - waveR);
            var falloff = Math.Max(0, 1 - delta / p.WaveWidth);
            var trail = dist < waveR ? 0.7 : 1.0;
            v = Math.Max(v, falloff * trail);
        }
        return v;
    }

    // --- Shimmer (per-pixel independent phase) --------------------------------
    //
    // The key to the "glorious shimmer" feel: every pixel has its own random
    // phase offset, so they all oscillate independently. The animation looks
    // like a starfield rather than flowing tissue.

    public readonly record struct ShimmerParams(double[] PhaseOffsets, double[] Speeds, double Floor);

    public static ShimmerParams ShimmerInit(Random rng, int size)
    {
        var n = size * size;
        var phaseOffsets = new double[n];
        var speeds = new double[n];
        for (var i = 0; i < n; i++)
        {
            phaseOffsets[i] = rng.NextDouble();
            speeds[i] = 0.7 + rng.NextDouble() * 1.6;
        }
        return new ShimmerParams(phaseOffsets, speeds, Floor: 0.10);
    }

    public static double Shimmer(int x, int y, int size, double t, in ShimmerParams p)
    {
        var idx = y * size + x;
        var s = Math.Sin(TwoPi * (t * p.Speeds[idx] + p.PhaseOffsets[idx]));
        var v = (s + 1) / 2;
        return p.Floor + v * (1 - p.Floor);
    }

    // --- helpers ---------------------------------------------------------------

    private static double Range(Random rng, double min, double max) => min + rng.NextDouble() * (max - min);

    private static T Pick<T>(Random rng, T[] arr) => arr[rng.Next(arr.Length)];
}

using Gabriel.Core.Personality;

namespace Gabriel.Engine.Sequence;

// Heuristic generator with VISUAL CHARACTER. Two rules that make each
// personality look distinct:
//
//   1. PALETTE is drawn from a curated set of vivid families (heat / cyber /
//      aurora / plasma / void / matrix / forge / grok / ...). The seed picks
//      one family; we expand its 2-4 gradient stops to a 16-entry palette.
//
//   2. PATTERN is one of five primitives (plasma, waves, spiral, pulse, shimmer)
//      picked by seed. Each primitive has its own animation grammar - plasma
//      morphs, waves flow directionally, spirals rotate, pulse rings out,
//      shimmer flickers per-pixel. That's what produces visual identity per
//      personality and removes the "fbm-everywhere blob" feel.
//
// The four layers (DNA / Traits / Context / Live) share the palette and pattern
// but each gets a distinct time window AND, for Live, a ConversationState
// modulation that compresses the palette window and adjusts intensity.
public sealed class GabrielSequenceGenerator : IGabrielSequenceGenerator
{
    private const int Size = 16;
    private const int FramesPerLayer = 16;

    public GabrielSequence Generate(
        long seed,
        ConversationState? state,
        string? patternOverride = null,
        string? paletteOverride = null)
    {
        // Palette: explicit override wins; otherwise seed-derived. Unknown
        // override names fall through to seed-derived behavior - see
        // PaletteTemplates.PickByName / SequenceCatalog.
        var paletteTemplate = PaletteTemplates.PickByName(paletteOverride)
            ?? PaletteTemplates.Pick(seed);
        var paletteName = paletteTemplate.Name;
        var palette = PaletteTemplates.ExpandTo(paletteTemplate, Size);

        // Seed a Random for pattern-parameter selection. Folding through xor +
        // a large prime keeps adjacent seeds from collapsing to the same params.
        // (The parameter RNG is still seed-derived even when the pattern KIND
        // is pinned - pinning chooses the primitive but the per-pattern params
        // stay fingerprinted to the seed.)
        var paramRng = new Random(unchecked((int)(seed ^ (seed >> 32) ^ 0xC2B2AE35)));
        var patternKind = SequenceCatalog.TryParsePattern(patternOverride)
            ?? (PatternKind)(Math.Abs(unchecked((int)(seed ^ (seed >> 32)))) % 5);
        var bundle = BuildPatternBundle(patternKind, paramRng);

        var frames = new Frame[GabrielSequence.FrameCount];

        // DNA Core (0..15): the base pattern at full cycle, full palette window.
        for (var i = 0; i < FramesPerLayer; i++)
        {
            var t = (double)i / FramesPerLayer;
            frames[i] = Render(bundle, t, paletteMin: 0, paletteMax: Size - 1, intensity: 1.0);
        }

        // Stable Traits (16..31): tilted sub-cycle, slightly shifted palette window.
        for (var i = 0; i < FramesPerLayer; i++)
        {
            var t = (double)i / FramesPerLayer + 0.25;
            frames[16 + i] = Render(bundle, t, paletteMin: 2, paletteMax: Size - 1, intensity: 0.95);
        }

        // Context (32..47): mid-detail; bigger phase offset for variety.
        for (var i = 0; i < FramesPerLayer; i++)
        {
            var t = (double)i / FramesPerLayer + 0.5;
            frames[32 + i] = Render(bundle, t, paletteMin: 1, paletteMax: Size - 2, intensity: 1.0);
        }

        // Live State (48..63): ConversationState-modulated palette window + intensity.
        var live = LiveStateProfile.From(state, Size);
        for (var i = 0; i < FramesPerLayer; i++)
        {
            var t = (double)i / FramesPerLayer + 0.75 + live.PhaseNudge;
            frames[48 + i] = Render(bundle, t, paletteMin: live.PaletteMin, paletteMax: live.PaletteMax, intensity: live.Intensity);
        }

        var metadata = new SequenceMetadata(
            Seed: seed,
            GeneratedAt: DateTimeOffset.UtcNow,
            StateSummary: $"pattern={patternKind.ToString().ToLowerInvariant()}, palette={paletteName}, {live.Summary}");

        return new GabrielSequence(GabrielSequence.SchemaVersion, palette, frames, metadata);
    }

    // --- pattern bundle --------------------------------------------------------

    private readonly record struct PatternBundle(
        PatternKind Kind,
        Patterns.PlasmaParams Plasma,
        Patterns.WavesParams Waves,
        Patterns.SpiralParams Spiral,
        Patterns.PulseParams Pulse,
        Patterns.ShimmerParams Shimmer);

    private static PatternBundle BuildPatternBundle(PatternKind kind, Random rng)
    {
        // Only initialize the chosen primitive - shimmer in particular allocates
        // size×size phase arrays which we'd rather skip when unused.
        return new PatternBundle(
            Kind: kind,
            Plasma: kind == PatternKind.Plasma ? Patterns.PlasmaInit(rng, Size) : default,
            Waves: kind == PatternKind.Waves ? Patterns.WavesInit(rng, Size) : default,
            Spiral: kind == PatternKind.Spiral ? Patterns.SpiralInit(rng, Size) : default,
            Pulse: kind == PatternKind.Pulse ? Patterns.PulseInit(rng, Size) : default,
            Shimmer: kind == PatternKind.Shimmer ? Patterns.ShimmerInit(rng, Size) : default);
    }

    private static double SamplePattern(in PatternBundle b, int x, int y, double t)
    {
        return b.Kind switch
        {
            PatternKind.Plasma  => Patterns.Plasma(x, y, t, b.Plasma),
            PatternKind.Waves   => Patterns.Waves(x, y, t, b.Waves),
            PatternKind.Spiral  => Patterns.Spiral(x, y, t, b.Spiral),
            PatternKind.Pulse   => Patterns.Pulse(x, y, t, b.Pulse),
            PatternKind.Shimmer => Patterns.Shimmer(x, y, Size, t, b.Shimmer),
            _ => 0.5,
        };
    }

    // --- frame rendering -------------------------------------------------------

    private static Frame Render(in PatternBundle bundle, double t, int paletteMin, int paletteMax, double intensity)
    {
        // Wrap t into [0, 1) for clean loop closure.
        t -= Math.Floor(t);

        var span = Math.Max(1, paletteMax - paletteMin + 1);
        var pixels = new byte[Frame.PixelCount];

        for (var y = 0; y < Frame.Height; y++)
        {
            for (var x = 0; x < Frame.Width; x++)
            {
                var v = SamplePattern(bundle, x, y, t);
                // Intensity pushes values toward the bright end - gives the
                // Live State window its "alive" feel without re-balancing the
                // palette window separately.
                v = Math.Clamp(v * intensity, 0, 1);
                var idx = paletteMin + (int)Math.Round(v * (span - 1));
                pixels[y * Frame.Width + x] = (byte)Math.Clamp(idx, paletteMin, paletteMax);
            }
        }
        return new Frame(pixels);
    }

    // --- live state profile ---------------------------------------------------

    private readonly record struct LiveStateProfile(
        int PaletteMin,
        int PaletteMax,
        double Intensity,
        double PhaseNudge,
        string Summary)
    {
        public static LiveStateProfile From(ConversationState? state, int paletteSize)
        {
            if (state is null)
            {
                return new LiveStateProfile(
                    PaletteMin: 0, PaletteMax: paletteSize - 1,
                    Intensity: 1.0, PhaseNudge: 0,
                    Summary: "no-state");
            }

            // Mood biases the palette window into a specific gradient zone.
            var (pMin, pMax, intensity) = state.Mood switch
            {
                Mood.Playful   => (paletteSize / 2,        paletteSize - 1,     1.10),  // bright, hot
                Mood.Venting   => (0,                      paletteSize / 2,     0.80),  // dark, dim
                Mood.Serious   => (paletteSize / 3,        2 * paletteSize / 3, 0.90),  // narrow midband
                Mood.Curious   => (1,                      paletteSize - 1,     1.05),  // wide, alive
                Mood.LowEnergy => (1,                      paletteSize / 2 - 1, 0.75),  // dark, sleepy
                _              => (1,                      paletteSize - 1,     1.0),
            };

            // Pinch the window further if the user has been sending shorts -
            // reads as slightly tense / restricted.
            if (state.ConsecutiveShortMessages >= 2)
            {
                var mid = (pMin + pMax) / 2;
                var halfSpan = Math.Max(1, (pMax - pMin) / 3);
                pMin = Math.Max(0, mid - halfSpan);
                pMax = Math.Min(paletteSize - 1, mid + halfSpan);
            }

            // Per-turn phase shove so the visible frames actually differ
            // between turns at the same mood - keeps the avatar feeling alive
            // rather than freezing on identical Live State frames.
            var phaseNudge = (state.TurnCount * 0.073 + state.LastUserTokenCount * 0.0013) % 1.0;

            var summary = $"mood={state.Mood.ToString().ToLowerInvariant()}, " +
                          $"turn={state.TurnCount}, lastTok={state.LastUserTokenCount}";

            return new LiveStateProfile(pMin, pMax, intensity, phaseNudge, summary);
        }
    }
}

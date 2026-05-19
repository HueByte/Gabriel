using Gabriel.Core.Personality;

namespace Gabriel.Engine.Sequence;

// Default heuristic generator. Each layer uses a distinct (seed offset, scale,
// octaves, palette range) signature so DNA / Traits / Context / Live State are
// visually distinguishable even though they share one palette.
//
// Live State is parameterized by ConversationState — mood biases the palette
// range, last-user-message length biases the noise scale, the
// consecutive-short-messages counter compresses the palette span.
public sealed class GabrielSequenceGenerator : IGabrielSequenceGenerator
{
    private const int PaletteSize = 16;

    public GabrielSequence Generate(long seed, ConversationState? state)
    {
        var palette = GeneratePalette(seed);
        var frames = new Frame[GabrielSequence.FrameCount];

        // DNA Core (0..15) — wide-scale features, full palette range, stable.
        var dnaSeed = FoldSeed(seed, salt: 0x1A2B3C);
        for (var i = 0; i < FrameLayers.FramesPerLayer; i++)
        {
            frames[i] = GenerateLayerFrame(
                frameSeed: dnaSeed + i,
                scale: 0.18,
                octaves: 3,
                paletteMin: 0,
                paletteMax: PaletteSize - 1,
                timePhase: 2 * Math.PI * i / FrameLayers.FramesPerLayer);
        }

        // Stable Traits (16..31) — finer features, mid-to-bright palette spread.
        var traitsSeed = FoldSeed(seed, salt: 0x4D5E6F);
        for (var i = 0; i < FrameLayers.FramesPerLayer; i++)
        {
            frames[16 + i] = GenerateLayerFrame(
                frameSeed: traitsSeed + i,
                scale: 0.26,
                octaves: 3,
                paletteMin: 3,
                paletteMax: PaletteSize - 1,
                timePhase: 2 * Math.PI * i / FrameLayers.FramesPerLayer);
        }

        // Context (32..47) — medium scale, slightly off-center palette emphasis.
        var contextSeed = FoldSeed(seed, salt: 0x708192);
        for (var i = 0; i < FrameLayers.FramesPerLayer; i++)
        {
            frames[32 + i] = GenerateLayerFrame(
                frameSeed: contextSeed + i,
                scale: 0.22,
                octaves: 4,
                paletteMin: 2,
                paletteMax: PaletteSize - 2,
                timePhase: 2 * Math.PI * i / FrameLayers.FramesPerLayer);
        }

        // Live State (48..63) — parameterized by ConversationState.
        var liveSeed = FoldSeed(seed, salt: 0xA3B4C5);
        var live = LiveStateProfile.From(state, PaletteSize);
        for (var i = 0; i < FrameLayers.FramesPerLayer; i++)
        {
            frames[48 + i] = GenerateLayerFrame(
                frameSeed: liveSeed + i + live.SeedNudge,
                scale: live.Scale,
                octaves: live.Octaves,
                paletteMin: live.PaletteMin,
                paletteMax: live.PaletteMax,
                timePhase: 2 * Math.PI * i / FrameLayers.FramesPerLayer);
        }

        var metadata = new SequenceMetadata(
            Seed: seed,
            GeneratedAt: DateTimeOffset.UtcNow,
            StateSummary: live.Summary);

        return new GabrielSequence(GabrielSequence.SchemaVersion, palette, frames, metadata);
    }

    // --- Palette ---------------------------------------------------------------

    // Narrow per-personality palette. Picks a base hue from the seed, then walks
    // through saturation + value variations so palette[0] is the quiescent
    // shadow and palette[N-1] is the brightest accent.
    private static Palette GeneratePalette(long seed)
    {
        // Distinct seed namespace so palette choice doesn't shadow frame seeds.
        var rng = new Random(unchecked((int)(seed ^ (seed >> 32) ^ 0xCAFEBABE)));

        var baseHue   = rng.NextDouble();
        var saturation = 0.45 + rng.NextDouble() * 0.40;
        var valueLo    = 0.10 + rng.NextDouble() * 0.18;
        var valueHi    = Math.Min(1.0, valueLo + 0.55 + rng.NextDouble() * 0.30);
        var hueDrift   = 0.05 + rng.NextDouble() * 0.06;

        var colors = new RgbColor[PaletteSize];
        for (var i = 0; i < PaletteSize; i++)
        {
            var t = i / (double)(PaletteSize - 1);
            var hue = (baseHue + (t - 0.5) * hueDrift + 1) % 1;
            var sat = Math.Clamp(saturation - t * 0.10, 0, 1);
            var val = valueLo + t * (valueHi - valueLo);
            colors[i] = RgbColor.FromHsv(hue, sat, val);
        }
        return new Palette(colors);
    }

    // --- Frame generation ------------------------------------------------------

    private static Frame GenerateLayerFrame(
        int frameSeed,
        double scale,
        int octaves,
        int paletteMin,
        int paletteMax,
        double timePhase)
    {
        var pixels = new byte[Frame.PixelCount];
        var span = Math.Max(1, paletteMax - paletteMin + 1);
        var phaseDx = Math.Cos(timePhase) * 0.6;
        var phaseDy = Math.Sin(timePhase) * 0.6;

        for (var y = 0; y < Frame.Height; y++)
        {
            for (var x = 0; x < Frame.Width; x++)
            {
                var nx = x * scale + phaseDx;
                var ny = y * scale + phaseDy;
                var n = Noise.Fbm(nx, ny, frameSeed, octaves);
                // fbm clusters around 0.5; remap so the tails reach 0 and 1.
                var t = Math.Clamp((n - 0.2) / 0.6, 0, 1);
                var idx = paletteMin + (int)Math.Round(t * (span - 1));
                pixels[y * Frame.Width + x] = (byte)Math.Clamp(idx, paletteMin, paletteMax);
            }
        }
        return new Frame(pixels);
    }

    // --- Helpers ---------------------------------------------------------------

    private static int FoldSeed(long seed, uint salt)
    {
        unchecked
        {
            var folded = (uint)(seed ^ (seed >> 32)) ^ salt;
            return (int)folded;
        }
    }

    // Per-turn parameters derived from the ConversationState. Bundled into a
    // small struct so the generator's main loop can stay tidy.
    private readonly record struct LiveStateProfile(
        double Scale,
        int Octaves,
        int PaletteMin,
        int PaletteMax,
        int SeedNudge,
        string Summary)
    {
        public static LiveStateProfile From(ConversationState? state, int paletteSize)
        {
            if (state is null)
            {
                return new LiveStateProfile(
                    Scale: 0.30,
                    Octaves: 3,
                    PaletteMin: 4,
                    PaletteMax: paletteSize - 4,
                    SeedNudge: 0,
                    Summary: "no-state");
            }

            // Mood biases the palette window: playful → bright tail; venting →
            // dark tail; serious → narrow midband; curious → wide window.
            var (pMin, pMax) = state.Mood switch
            {
                Mood.Playful   => (paletteSize / 2,        paletteSize - 1),
                Mood.Venting   => (0,                      paletteSize / 2),
                Mood.Serious   => (paletteSize / 3,        2 * paletteSize / 3),
                Mood.Curious   => (2,                      paletteSize - 2),
                Mood.LowEnergy => (1,                      paletteSize / 2 - 1),
                _              => (3,                      paletteSize - 3),
            };

            // Long messages → larger features (lower scale). Short → smaller
            // features (higher scale, more grain). LastUserTokenCount is char/4.
            var scale = state.LastUserTokenCount switch
            {
                <= 5    => 0.42,
                <= 20   => 0.34,
                <= 60   => 0.28,
                <= 150  => 0.22,
                _       => 0.18,
            };

            // Consecutive shorts pinch the palette window — the avatar reads as
            // less varied, slightly tense.
            if (state.ConsecutiveShortMessages >= 2)
            {
                var mid = (pMin + pMax) / 2;
                var halfSpan = Math.Max(1, (pMax - pMin) / 3);
                pMin = Math.Max(0, mid - halfSpan);
                pMax = Math.Min(paletteSize - 1, mid + halfSpan);
            }

            // Octaves track engagement — curious / venting get more detail.
            var octaves = state.Mood is Mood.Curious or Mood.Venting ? 4 : 3;

            // Tiny per-turn seed perturbation so the visible state actually
            // changes between turns even at the same mood.
            var nudge = state.TurnCount * 31 + state.LastUserTokenCount;

            var summary = $"mood={state.Mood.ToString().ToLowerInvariant()}, " +
                          $"turn={state.TurnCount}, " +
                          $"lastTok={state.LastUserTokenCount}, " +
                          $"shorts={state.ConsecutiveShortMessages}";

            return new LiveStateProfile(scale, octaves, pMin, pMax, nudge, summary);
        }
    }
}

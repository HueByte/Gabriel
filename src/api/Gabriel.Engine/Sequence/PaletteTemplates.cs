namespace Gabriel.Engine.Sequence;

// Vivid palette families, mirroring the webapp's pulse/palettes.ts shape so
// the Gabriel Sequence stays visually coherent with the procedural avatar
// already on screen. Each template is 2-4 gradient stops (RGB); we expand to
// 16 palette entries by sampling the gradient evenly.
//
// Per-seed selection picks ONE template - keeps each personality's palette
// recognizable across regenerations of its Live State.
internal static class PaletteTemplates
{
    public sealed record Template(string Name, RgbColor[] Stops);

    // Stops are intentionally bold: the darkest stop reaches near-black, the
    // brightest stop reaches near-white-tinted, with a fully saturated mid.
    // This is what gives the avatar real "vivid color" instead of the muted
    // monochromatic palettes the first generator produced.
    public static readonly Template[] All =
    {
        new("heat",    new[] { Rgb( 40,   0,  10), Rgb(255, 140,  20), Rgb(255, 255, 200) }),
        new("ice",     new[] { Rgb( 20,  40,  90), Rgb(120, 200, 255), Rgb(240, 250, 255) }),
        new("plasma",  new[] { Rgb( 60,   0,  80), Rgb(220,  40, 160), Rgb(255, 220, 240) }),
        new("matrix",  new[] { Rgb( 10,  40,  10), Rgb( 60, 220,  80), Rgb(220, 255, 200) }),
        new("sunset",  new[] { Rgb(100,  20,  90), Rgb(240,  90,  60), Rgb(255, 220, 140) }),
        new("ocean",   new[] { Rgb( 15,  50,  90), Rgb( 40, 160, 180), Rgb(200, 250, 240) }),
        new("aurora",  new[] { Rgb( 30,  20,  60), Rgb( 60, 200, 180), Rgb(200, 140, 240) }),
        new("rose",    new[] { Rgb( 60,  20,  40), Rgb(240,  90, 140), Rgb(255, 220, 230) }),
        new("cyber",   new[] { Rgb( 40,   0,  80), Rgb(200,   0, 220), Rgb( 40, 220, 255) }),
        new("amber",   new[] { Rgb( 80,  30,  10), Rgb(255, 180,  60) }),
        new("lime",    new[] { Rgb( 40,  60,   0), Rgb(180, 240,  40) }),
        new("sakura",  new[] { Rgb( 80,  40,  60), Rgb(255, 180, 200) }),
        new("mono",    new[] { Rgb(  0,   0,   0), Rgb(180, 180, 180), Rgb(255, 255, 255) }),
        new("void",    new[] { Rgb(  0,   0,   0), Rgb( 80,  40, 120), Rgb(200, 160, 255) }),
        new("forge",   new[] { Rgb( 20,  10,  30), Rgb(220,  80,  40), Rgb(255, 230, 120) }),
        new("grok",    new[] { Rgb( 50,  20,  60), Rgb(200,  40, 130), Rgb(255, 100, 130), Rgb(255, 200, 160) }),
    };

    public static Template Pick(long seed)
    {
        // Use a folded version of the seed dedicated to palette choice so it
        // doesn't shadow frame-pattern seeds.
        unchecked
        {
            var mixed = (uint)(seed ^ (seed >> 32) ^ 0x9E3779B1);
            return All[(int)(mixed % (uint)All.Length)];
        }
    }

    // Look up a template by name (case-insensitive). Returns null when the
    // name doesn't match any registered template - callers should fall back
    // to seed-derived pick. Used to honor explicit Project / Conversation
    // PaletteOverride values.
    public static Template? PickByName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var key = name.Trim();
        for (var i = 0; i < All.Length; i++)
        {
            if (string.Equals(All[i].Name, key, StringComparison.OrdinalIgnoreCase))
                return All[i];
        }
        return null;
    }

    // Expand a 2-4 stop template to a 16-entry palette by sampling the
    // gradient evenly. palette[0] is the quiescent shadow, palette[15] is
    // the brightest accent.
    public static Palette ExpandTo(Template template, int size = 16)
    {
        var colors = new RgbColor[size];
        for (var i = 0; i < size; i++)
        {
            var t = (double)i / (size - 1);
            colors[i] = SampleGradient(template.Stops, t);
        }
        return new Palette(colors);
    }

    private static RgbColor SampleGradient(RgbColor[] stops, double t)
    {
        t = Math.Clamp(t, 0, 1);
        if (stops.Length == 1) return stops[0];

        var segments = stops.Length - 1;
        var segLen = 1.0 / segments;
        var i = Math.Min(segments - 1, (int)Math.Floor(t / segLen));
        var k = (t - i * segLen) / segLen;

        var a = stops[i];
        var b = stops[i + 1];
        return new RgbColor(
            (byte)Math.Clamp((int)Math.Round(a.R + (b.R - a.R) * k), 0, 255),
            (byte)Math.Clamp((int)Math.Round(a.G + (b.G - a.G) * k), 0, 255),
            (byte)Math.Clamp((int)Math.Round(a.B + (b.B - a.B) * k), 0, 255));
    }

    private static RgbColor Rgb(int r, int g, int b) => new((byte)r, (byte)g, (byte)b);
}

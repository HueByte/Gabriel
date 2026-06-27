using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gabriel.Engine.Tools.Colors;

// Convert a color between hex, rgb(), and hsl() notations. A pure function of
// its arguments - no I/O, no dependency. Exists because the channel math (and
// especially RGB<->HSL) is fiddly to do by hand and easy to get subtly wrong.
public sealed partial class ColorConvertTool : ITool
{
    private const int MaxValueLength = 200;

    public string Name => "color_convert";

    public string Description =>
        "Convert a color between hex, rgb(), and hsl() notations. " +
        "USE THIS for color conversions - reading a hex value as rgb, getting the " +
        "hsl of a color - instead of doing the channel math by hand. Input may be " +
        "hex (#rgb, #rgba, #rrggbb, #rrggbbaa), rgb()/rgba(), or hsl()/hsla(); an " +
        "alpha channel is preserved. Give 'to' for a single format, or omit it to " +
        "get all three. NOT for named colors like 'red'.";

    public string ParametersJsonSchema => """
        {
          "type": "object",
          "properties": {
            "value": {
              "type": "string",
              "description": "The color to convert: hex (#rrggbb), rgb(r, g, b[, a]), or hsl(h, s%, l%[, a])."
            },
            "to": {
              "type": "string",
              "enum": ["hex", "rgb", "hsl"],
              "description": "Target notation. Omit to return all three."
            }
          },
          "required": ["value"]
        }
        """;

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        try
        {
            var (value, to) = ReadArgs(argumentsJson);
            var c = Parse(value);

            if (to is null)
            {
                var block = $"hex: {ToHex(c)}\nrgb: {ToRgb(c)}\nhsl: {ToHsl(c)}";
                return Task.FromResult(block);
            }

            var converted = to switch
            {
                "hex" => ToHex(c),
                "rgb" => ToRgb(c),
                _ => ToHsl(c),
            };
            return Task.FromResult($"{value.Trim()} → {converted}");
        }
        catch (ColorException ex)
        {
            return Task.FromResult($"Error: {ex.Message}");
        }
    }

    private static (string Value, string? To) ReadArgs(string argumentsJson)
    {
        JsonDocument doc;
        try { doc = JsonDocument.Parse(argumentsJson); }
        catch (JsonException) { throw new ColorException("arguments were not valid JSON."); }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("value", out var valueEl) || valueEl.ValueKind != JsonValueKind.String)
                throw new ColorException("'value' is required and must be a string.");
            var value = valueEl.GetString() ?? "";
            if (string.IsNullOrWhiteSpace(value))
                throw new ColorException("'value' cannot be empty.");
            if (value.Length > MaxValueLength)
                throw new ColorException($"'value' is too long (max {MaxValueLength} characters).");

            string? to = null;
            if (root.TryGetProperty("to", out var toEl) && toEl.ValueKind != JsonValueKind.Null)
            {
                if (toEl.ValueKind != JsonValueKind.String)
                    throw new ColorException("'to' must be a string.");
                to = toEl.GetString()!.ToLowerInvariant();
                if (to is not ("hex" or "rgb" or "hsl"))
                    throw new ColorException($"'to' must be hex, rgb, or hsl (got '{toEl.GetString()}').");
            }

            return (value, to);
        }
    }

    // ---- parsing ----

    private readonly record struct Rgba(int R, int G, int B, double A);

    private static Rgba Parse(string raw)
    {
        var s = raw.Trim();
        var lower = s.ToLowerInvariant();
        if (lower.StartsWith("rgb")) return ParseRgb(s);
        if (lower.StartsWith("hsl")) return ParseHsl(s);
        return ParseHex(s); // with or without leading '#'
    }

    private static Rgba ParseHex(string s)
    {
        var h = s.StartsWith('#') ? s[1..] : s;
        if (h.Length is not (3 or 4 or 6 or 8) || !h.All(Uri.IsHexDigit))
            throw new ColorException($"'{s}' is not a valid color. Use hex (#rrggbb), rgb(...), or hsl(...).");
        if (h.Length is 3 or 4)
            h = string.Concat(h.Select(c => new string(c, 2))); // #rgb -> #rrggbb

        var r = Convert.ToInt32(h.Substring(0, 2), 16);
        var g = Convert.ToInt32(h.Substring(2, 2), 16);
        var b = Convert.ToInt32(h.Substring(4, 2), 16);
        var a = h.Length == 8 ? Convert.ToInt32(h.Substring(6, 2), 16) / 255.0 : 1.0;
        return new Rgba(r, g, b, a);
    }

    private static Rgba ParseRgb(string s)
    {
        var m = RgbPattern().Match(s);
        if (!m.Success) throw new ColorException($"'{s}' is not a valid rgb()/rgba() color.");
        var r = Channel(m.Groups[1].Value);
        var g = Channel(m.Groups[2].Value);
        var b = Channel(m.Groups[3].Value);
        var a = m.Groups[4].Success ? AlphaValue(m.Groups[4].Value) : 1.0;
        return new Rgba(r, g, b, a);
    }

    private static Rgba ParseHsl(string s)
    {
        var m = HslPattern().Match(s);
        if (!m.Success) throw new ColorException($"'{s}' is not a valid hsl()/hsla() color.");
        var h = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        var sat = Percent(m.Groups[2].Value);
        var lig = Percent(m.Groups[3].Value);
        var a = m.Groups[4].Success ? AlphaValue(m.Groups[4].Value) : 1.0;
        var (r, g, b) = HslToRgb(h, sat, lig);
        return new Rgba(r, g, b, a);
    }

    private static int Channel(string raw)
    {
        var v = int.Parse(raw, CultureInfo.InvariantCulture);
        if (v is < 0 or > 255) throw new ColorException($"rgb channel {v} is out of range (0-255).");
        return v;
    }

    private static double Percent(string raw)
    {
        var v = double.Parse(raw.TrimEnd('%'), CultureInfo.InvariantCulture);
        if (v is < 0 or > 100) throw new ColorException($"hsl percentage {v} is out of range (0-100).");
        return v;
    }

    private static double AlphaValue(string raw)
    {
        var v = double.Parse(raw, CultureInfo.InvariantCulture);
        if (v is < 0 or > 1) throw new ColorException($"alpha {v} is out of range (0-1).");
        return v;
    }

    // ---- conversions ----

    private static (int H, int S, int L) RgbToHsl(int r, int g, int b)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        var max = Math.Max(rd, Math.Max(gd, bd));
        var min = Math.Min(rd, Math.Min(gd, bd));
        double h = 0, s = 0, l = (max + min) / 2;

        if (max > min)
        {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            if (max == rd) h = (gd - bd) / d + (gd < bd ? 6 : 0);
            else if (max == gd) h = (bd - rd) / d + 2;
            else h = (rd - gd) / d + 4;
            h /= 6;
        }

        return ((int)Math.Round(h * 360), (int)Math.Round(s * 100), (int)Math.Round(l * 100));
    }

    private static (int R, int G, int B) HslToRgb(double hDeg, double sPct, double lPct)
    {
        var h = (((hDeg % 360) + 360) % 360) / 360.0;
        var s = sPct / 100.0;
        var l = lPct / 100.0;

        double r, g, b;
        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = HueToChannel(p, q, h + 1.0 / 3);
            g = HueToChannel(p, q, h);
            b = HueToChannel(p, q, h - 1.0 / 3);
        }

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));

        static double HueToChannel(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }

    // ---- formatting ----

    private static string ToHex(Rgba c)
    {
        var hex = $"#{c.R:x2}{c.G:x2}{c.B:x2}";
        if (c.A < 1.0) hex += ((int)Math.Round(c.A * 255)).ToString("x2");
        return hex;
    }

    private static string ToRgb(Rgba c) =>
        c.A < 1.0 ? $"rgba({c.R}, {c.G}, {c.B}, {Alpha(c.A)})" : $"rgb({c.R}, {c.G}, {c.B})";

    private static string ToHsl(Rgba c)
    {
        var (h, s, l) = RgbToHsl(c.R, c.G, c.B);
        return c.A < 1.0 ? $"hsla({h}, {s}%, {l}%, {Alpha(c.A)})" : $"hsl({h}, {s}%, {l}%)";
    }

    private static string Alpha(double a) => Math.Round(a, 2).ToString(CultureInfo.InvariantCulture);

    [GeneratedRegex(@"^rgba?\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*(?:,\s*(\d*\.?\d+)\s*)?\)$", RegexOptions.IgnoreCase)]
    private static partial Regex RgbPattern();

    [GeneratedRegex(@"^hsla?\(\s*(\d*\.?\d+)\s*,\s*(\d*\.?\d+\s*%?)\s*,\s*(\d*\.?\d+\s*%?)\s*(?:,\s*(\d*\.?\d+)\s*)?\)$", RegexOptions.IgnoreCase)]
    private static partial Regex HslPattern();

    // Validation failures the agent should read as observations, kept distinct
    // from genuine bugs (which surface normally).
    private sealed class ColorException : Exception
    {
        public ColorException(string message) : base(message) { }
    }
}

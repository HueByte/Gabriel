namespace Gabriel.Engine.Sequence;

// Single RGB triplet, 8 bits per channel. Value type because palette entries
// are inlined in arrays and benefit from no heap pressure.
public readonly record struct RgbColor(byte R, byte G, byte B)
{
    public static RgbColor FromHsv(double hue, double saturation, double value)
    {
        // Standard HSV → RGB conversion. Hue in [0,1), the other two in [0,1].
        var h = (hue % 1.0 + 1.0) % 1.0 * 6.0;
        var c = value * saturation;
        var x = c * (1 - Math.Abs(h % 2 - 1));
        var m = value - c;
        var (r, g, b) = (int)Math.Floor(h) switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return new RgbColor(
            (byte)Math.Clamp((r + m) * 255, 0, 255),
            (byte)Math.Clamp((g + m) * 255, 0, 255),
            (byte)Math.Clamp((b + m) * 255, 0, 255));
    }
}

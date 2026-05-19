namespace Gabriel.Engine.Sequence;

// A single 16×16 frame, palette-indexed (each pixel is a `byte` index into
// the Sequence's Palette.Colors). Row-major: Pixels[y * Width + x].
public sealed record Frame(byte[] Pixels)
{
    public const int Width = 16;
    public const int Height = 16;
    public const int PixelCount = Width * Height;

    public byte At(int x, int y) => Pixels[y * Width + x];
}

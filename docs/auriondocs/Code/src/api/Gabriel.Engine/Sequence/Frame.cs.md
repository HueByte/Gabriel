# Frame

> **File:** `src/api/Gabriel.Engine/Sequence/Frame.cs`  
> **Kind:** record

```csharp
public sealed record Frame(byte[] Pixels)
{
    public const int Width = 16;
    public const int Height = 16;
    public const int PixelCount = Width * Height;

    public byte At(int x, int y) => Pixels[y * Width + x];
}
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Pixels` | `byte[]` | — |


Frame represents a 16×16 tile where every pixel stores a byte index into a Palette, enabling color resolution to be performed by a separate Palette at render time. The At(x, y) method gives quick access to the pixel index at coordinates, using the standard row-major layout Pixels[y * Width + x].

## Remarks
Because the color information is not embedded, a single Frame can be used with many Palettes, supporting palette swaps and theme changes without altering geometry. This separation of concerns makes it easy to reuse frames across sprites or tilesets while mapping to different color schemes at render time. Being a sealed, single-property record gives Frame value equality semantics, which helps when frames are stored in collections or compared for changes.

## Example
```csharp
Frame frame = new Frame(pixels);
var index = frame.At(5, 7);
var color = palette[index];
```

## Notes
- At() does not perform bounds checks; ensure 0 <= x < Width and 0 <= y < Height before calling At.
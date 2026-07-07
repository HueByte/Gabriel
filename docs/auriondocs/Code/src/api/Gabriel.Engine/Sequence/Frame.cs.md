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


Represents a compact, immutable frame of pixel data for a 16×16 image, stored as palette indices. It is a value object (record) with a single property, Pixels, containing 256 bytes where each byte is an index into a Palette's Colors. The frame's fixed dimensions are exposed by Width, Height, and PixelCount constants, and pixels are stored in row-major order. Accessing a specific pixel uses At(x, y), which computes the offset y * Width + x and returns the corresponding byte from Pixels. This abstraction is useful whenever you need to shuttle small, palette-based frames around a sequence of colors without dealing with raw color values directly.

## Remarks
Frame centralizes the layout concerns (size, indexing) in one place, ensuring consistent interpretation across consumers that share the same Palette. As a record, Frame benefits from value-based equality semantics, easy comparisons, and safe copying. The Pixels array is the sole data payload; higher-level rendering or color interpretation is performed by Palette and rendering code.

## Notes
- At(x, y) does not guard against out-of-bounds coordinates; callers must ensure 0 ≤ x < Width and 0 ≤ y < Height to avoid IndexOutOfRangeException.
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


Frame is an immutable, 16×16 frame that stores pixel data as palette indices. The Pixels array holds 256 bytes, each representing a index into a Palette, and the At(x, y) method retrieves the index for a given coordinate using row-major ordering (y * Width + x). This abstraction separates pixel data from color data and provides a straightforward way to render a frame by looking up colors in a Palette.

## Remarks
Frame encapsulates a fixed-size, palette-indexed pixel block and exposes a minimal API (At) for access. It serves as the primitive unit in a sequence of frames, allowing rendering code to translate indices into colors via a Palette without duplicating color data or logic.

## Example
```csharp
// Create a 16×16 frame with all indices set to 0
var frame = new Frame(new byte[Frame.PixelCount]);

// Read the palette index for pixel at (5, 7)
byte index = frame.At(5, 7);

// Render that pixel by looking up the color in a Palette
int color = palette[index];
```

## Notes
- At does not perform explicit bounds checking; callers must ensure 0 <= x < Width and 0 <= y < Height.
- The Pixels length is expected to be 256 (Frame.PixelCount); the constructor does not validate length, so mis-sized arrays may lead to incorrect results or runtime exceptions.
- Frame is a sealed record, emphasizing immutability and value-based equality, making it a stable unit for rendering pipelines and caching.
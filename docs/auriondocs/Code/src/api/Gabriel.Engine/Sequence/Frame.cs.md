# Frame

> **File:** `src/api/Gabriel.Engine/Sequence/Frame.cs`  
> **Kind:** record

A compact container for a single 16×16, palette-indexed image frame. Each pixel is stored as a byte that indexes into a Sequence's palette; use this type when representing small frames or sprite tiles where colors are looked up via a Palette rather than stored directly.

## Remarks
This record wraps a raw byte array in row-major order (Pixels[y * Width + x]) and exposes a convenience At(x, y) accessor. It intentionally stores palette indices (not color values) so it integrates with a Palette/Sequence model where colors are resolved separately. The record does not validate or copy the provided array — it simply holds a reference to the caller-supplied buffer.

## Example
```csharp
// Create an empty frame, set one pixel, read it back
var pixels = new byte[Frame.PixelCount]; // 16 * 16 == 256
pixels[5 * Frame.Width + 3] = 7; // set pixel at (3,5) to palette index 7
var frame = new Frame(pixels);
byte index = frame.At(3, 5); // 7
```

## Notes
- The Pixels array is expected to contain exactly Frame.PixelCount entries; methods (including At) will throw IndexOutOfRangeException if given out-of-range coordinates or if the array is too short.
- Pixel values are palette indices — you must map them through the Sequence's Palette.Colors to obtain actual colors.
- The record stores the array by reference and does not make a defensive copy. Mutating the Pixels array after constructing a Frame will change the frame's contents and may introduce race conditions in concurrent scenarios.
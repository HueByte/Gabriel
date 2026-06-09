# Frame

> **File:** `src/api/Gabriel.Engine/Sequence/Frame.cs`  
> **Kind:** record

Represents a single 16×16, palette‑indexed image where each pixel is stored as a byte index into a Sequence's palette. Use this when you need a compact, fixed-size frame for animations or sprite data; the record exposes constants for Width, Height and PixelCount and provides At(x,y) to read a pixel by coordinates.

## Remarks
This type encodes pixels as raw byte indices (0–255) rather than color values, keeping storage dense and cheap to copy around. It is intended to be used together with a Palette (Sequence.Palette.Colors) that maps these indices to actual colors. The internal pixel buffer uses row‑major ordering: Pixels[y * Width + x]. The record wrapper gives a lightweight, nominally value-like container, but the backing byte[] remains a mutable reference.

## Example
```csharp
// Create an empty (transparent/zeroed) frame and set or read pixels
var data = new byte[Frame.PixelCount]; // must be 16*16
var frame = new Frame(data);
byte index = frame.At(3, 2); // read pixel at x=3, y=2
```

## Notes
- The Pixels array must contain exactly Frame.PixelCount elements; callers are responsible for providing the correctly sized buffer (no defensive length checks are performed by the constructor). Bounds errors in At(x,y) will throw if x/y are out of range or the array is incorrect.
- The Pixels array is stored by reference and is mutable: modifying the array after constructing a Frame changes the frame's contents. This also means Frame instances are not inherently thread‑safe.
- Equality for the generated record compares the Pixels field by reference (the array itself), which can be surprising if you expect element-wise/frame content equality.
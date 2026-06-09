# FrameLayer.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`

## Contents

- [FrameLayers](#framelayers)
- [FrameLayer](#framelayer)

---

## FrameLayers

> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** class

Utilities for mapping between absolute frame indices and their logical "layers." Use this when you need to determine which fixed-size layer a given frame belongs to or when you need the inclusive start/end frame indices for a specific layer.

## Remarks
This class centralizes the layer size (FramesPerLayer) and the number of layers (LayerCount) so callers share a single source of truth for how frames are grouped. It provides two small helpers: Of computes the layer index by integer division, and Range returns the inclusive start/end frame indices for a layer.

## Example
```csharp
// Determine which layer frame 18 belongs to and the range of that layer
var layer = FrameLayers.Of(18);              // layer for frame 18
var (start, end) = FrameLayers.Range(layer); // start == 16, end == 31
```

## Notes
- Of(int frameIndex) uses integer division (floor) to map frames to layers; frameIndex 0..15 => layer 0, 16..31 => layer 1, etc.
- Neither Of nor Range validate their inputs. Passing a negative frameIndex or a FrameLayer value outside the intended range can produce negative or out-of-range results.
- Range returns an inclusive End value (Start..End contains FramesPerLayer frames).
- LayerCount is declared for callers' reference but is not enforced by the methods.

---

## FrameLayer

> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** enum

Represents the four logical layers that partition a 64-frame sequence. Each enum value corresponds to a contiguous 16-frame range used to classify frames by lifespan and semantics: DnaCore (frames 0–15), StableTraits (16–31), Context (32–47), and LiveState (48–63). Reach for this enum when you need to map a numeric frame index to its lifecycle layer or to reason about frame scope.

## Remarks
This enum encodes a conceptual layering that separates immutable identity (DnaCore), long-term preferences (StableTraits), medium-term accumulated reactions (Context), and the current emotional or transient state (LiveState). The intended usage is to compute the layer from a frame index using integer division by 16 and then cast to FrameLayer; this keeps frame-handling logic simple and consistent across the codebase.

## Example
```csharp
int index = 27; // some frame index in 0..63
if (index < 0 || index >= 64) throw new ArgumentOutOfRangeException(nameof(index));
FrameLayer layer = (FrameLayer)(index / 16); // yields FrameLayer.StableTraits

// switch on layer
switch (layer)
{
    case FrameLayer.DnaCore:      /* handle immutable identity frames */ break;
    case FrameLayer.StableTraits: /* handle long-term preferences */ break;
    case FrameLayer.Context:      /* handle medium-term context */ break;
    case FrameLayer.LiveState:    /* handle current emotional state */ break;
}
```

## Notes
- Integer division is used to map indices to layers; ensure you use (index / 16) not floating-point division.
- Valid frame indices are 0..63; casting (FrameLayer)(index / 16) can produce an invalid enum value if the index is outside that range.
- The numeric values are fixed (0–3) and rely on 16-frame ranges; changing the underlying partition size requires coordinated updates wherever indices are converted to layers.

---
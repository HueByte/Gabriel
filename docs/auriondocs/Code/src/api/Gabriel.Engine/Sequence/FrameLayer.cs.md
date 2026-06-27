# FrameLayer.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`

## Contents

- [FrameLayers](#framelayers)
- [FrameLayer](#framelayer)

---

## FrameLayers

> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** class

Provides helpers for mapping between absolute frame indices and logical frame layers. Use this when you need to determine which layer a given frame belongs to or to obtain the inclusive start/end frame indices for a particular layer. The class centralizes the layer sizing constants (FramesPerLayer = 16) and total layer count (LayerCount = 4).

## Remarks
FrameLayers encapsulates the simple convention that frames are grouped into fixed-size layers of 16 frames each. It exists to keep the mapping logic and size constants in one place so callers don't repeatedly hard-code division and offset arithmetic. The Of method performs integer division and casts the result to the FrameLayer enum, while Range computes the inclusive start and end indices for a layer.

## Example
```csharp
// Determine which layer contains frame 37
var layer = FrameLayers.Of(37); // frame 37 -> layer (37 / 16) = 2

// Get the start/end frames for that layer
var (start, end) = FrameLayers.Range(layer); // start = 32, end = 47

// Iterate all frames in a layer
for (int i = start; i <= end; i++)
{
    // process frame i
}
```

## Notes
- The methods do not validate inputs: passing a negative frameIndex or an enum value outside the expected range may produce unexpected results because Of uses integer division and a direct cast to FrameLayer.
- Range returns an inclusive end index (Start .. End). FramesPerLayer is 16, so End = Start + 15. If your code expects half-open ranges, adjust accordingly.

---

## FrameLayer

> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** enum

Represents the four logical layers used to partition a 64-frame memory/sequence into groups of 16 frames each. Use this enum when you need to categorize a frame index by its intended persistence and update cadence: DnaCore (frames 0..15) is immutable identity data, StableTraits (16..31) holds long-term preferences, Context (32..47) stores medium-term accumulated reactions, and LiveState (48..63) contains the current emotional/stateful values.

## Remarks
This enum encodes a conceptual separation of memory/state into layers with different lifetimes and update frequencies. The design makes it easy to decide how and when to read, write, or decay values by layer instead of by individual frame indices; typical usage is to compute the layer from a numeric frame index and then apply layer-specific logic (persistence, update rate, serialization rules, etc.).

## Example
```csharp
int frameIndex = 37;
if (frameIndex < 0 || frameIndex >= 64) throw new ArgumentOutOfRangeException(nameof(frameIndex));
FrameLayer layer = (FrameLayer)(frameIndex / 16); // integer division yields 2 -> FrameLayer.Context
```

## Notes
- The enum values are simple integers (0..3) and are not bit flags.
- Casting an integer outside 0..3 to this enum will produce an enum value with that underlying integer; validate frame indices (0..63) before casting.
- Each layer covers exactly 16 frames; ranges in the code comments are inclusive (e.g., DnaCore = frames 0..15).

---
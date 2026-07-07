# FrameLayer.cs

> **Source:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`

## Contents

- [FrameLayers](#framelayers)
- [FrameLayer](#framelayer)

---

## FrameLayers
> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** class

```csharp
public static class FrameLayers
```


FrameLayers is a small, static helper that partitions a flat sequence of frames into fixed layers. It defines two constants—FramesPerLayer (16) and LayerCount (4)—and provides Of to map a frame index to its corresponding FrameLayer, as well as Range to obtain the inclusive start and end indices for a given layer. This utility is useful whenever you need to operate on frames in fixed-size chunks rather than treating every index independently.

## Remarks
FrameLayers codifies the segmentation strategy used across the engine, ensuring consistent interpretation of frame indices. Centralizing the layer size and the mapping logic prevents scattered magic numbers and makes the relationship between a frame index and its layer explicit. The Of method yields a FrameLayer value by dividing the index by FramesPerLayer, so callers should ensure the input is within the valid total range; otherwise the resulting FrameLayer may reference an undefined or out-of-range value. Range provides the inclusive Start/End bounds for a layer, which is convenient for loops that process all frames within that layer.

## Notes
- Input validation: Of does not perform bounds checks; callers must ensure 0 <= frameIndex < LayerCount * FramesPerLayer.
- Inclusive range: Range returns (start, end) with end = start + FramesPerLayer - 1; loops should use <= End or adapt to the inclusive range.
- Maintenance: If FramesPerLayer or LayerCount is changed, adjust dependent code accordingly; keeping these in one place reduces drift.

---

## FrameLayer
> **File:** `src/api/Gabriel.Engine/Sequence/FrameLayer.cs`  
> **Kind:** enum

```csharp
public enum FrameLayer
{
    DnaCore       = 0,
    StableTraits  = 1,
    Context       = 2,
    LiveState     = 3,
}
```


FrameLayer is an enum that categorizes the 64-frame timeline into four conceptual layers: DnaCore (immutable identity), StableTraits (long-term preferences), Context (medium-term reactions), and LiveState (current state). The layer for a given frame index is computed as (index / 16) cast to FrameLayer, enabling layer-aware logic without hard-coding index ranges.

## Remarks
This abstraction captures how different aspects of an entity evolve on different timescales. By mapping a frame to its layer, systems can decide how aggressively to update, cache, or serialize data, and can keep responsibilities separated (identity vs. traits vs. context vs. live state). It also provides a simple, readable way to reason about behavior that should only apply to specific portions of the 64-frame window.

## Example
```csharp
int frameIndex = 23;
FrameLayer layer = (FrameLayer)(frameIndex / 16); // StableTraits
```

## Notes
- Casting an arbitrary int to FrameLayer assumes the index is within 0..63; validate your input before casting.
- The numeric values align with the four layers: DnaCore=0, StableTraits=1, Context=2, LiveState=3.

---
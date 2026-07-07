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


FrameLayers is a small static helper that partitions a linear sequence of frames into four fixed layers of sixteen frames each. It exposes FramesPerLayer and LayerCount, and provides Of(frameIndex) to determine the Layer for a given global frame index, and Range(layer) to compute the inclusive start and end frame indices for a specific Layer.

## Remarks
FrameLayers centralizes the notion of frame layering, removing magic numbers from clients and ensuring consistent layer-based calculations across the codebase. It serves as a clean boundary between raw frame indices and layer-based processing, making it easy to perform per-layer iterates or to compute per-layer bounds without duplicating arithmetic.

## Notes
- No input validation; negative or out-of-range frameIndex values can yield undefined FrameLayer values when casting, and Range may return invalid bounds for such inputs.
- If the total number of frames changes, update FramesPerLayer and LayerCount accordingly, or else Range results may refer to non-existent frames.

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


FrameLayer is an enum that partitions the 64-frame timeline into four conceptual layers: DnaCore for immutable identity, StableTraits for long-term preferences, Context for medium-term accumulated reactions, and LiveState for the current emotional state recalculated each turn. The layer corresponding to a given frame index is computed as (index / 16) cast to FrameLayer. This abstraction lets callers write layer-aware logic without referencing exact frame ranges, enabling clearer separation of concerns and efficient, layer-specific processing.

## Remarks
FrameLayer serves as a lightweight categorization that underpins state management in the system. It clarifies responsibilities across components by ensuring operations target the correct abstraction (e.g., avoiding mutation of DnaCore during normal updates, isolating long-term drift from immediate state). It also supports optimizations by allowing batch work per layer.

## Example
```csharp
int frameIndex = 33;
FrameLayer layer = (FrameLayer)(frameIndex / 16); // Context
```

## Notes
- Out-of-range safety: valid frame indices are 0..63. Values outside map to an enum value outside the defined set; callers should clamp/validate before casting.


---
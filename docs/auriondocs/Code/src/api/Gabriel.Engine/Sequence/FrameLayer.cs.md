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


FrameLayers is a compact static helper that defines the partitioning of frames into layers and exposes two helpers to work with that partitioning. Use Of(frameIndex) to map a frame index to its corresponding FrameLayer, and Range(layer) to obtain the inclusive start and end indices for a given layer.

## Remarks
FrameLayers centralizes the framing logic, tying the concept of layers to the FrameLayer enum and ensuring consistent calculations across the codebase. By consolidating constants (FramesPerLayer, LayerCount) and simple arithmetic into a single place, it reduces duplication and the risk of off-by-one errors when iterating or validating layer ranges. The static nature of the class makes it a readily accessible utility for any component that processes frames in groups, enabling straightforward, batched operations over each layer.

## Notes
- Range uses an inclusive end bound; if you need an exclusive upper bound, adjust accordingly.
- Of relies on integer division; negative frameIndex maps to a lower layer due to truncation, and there is no input validation performed.

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


FrameLayer encodes the logical tier of the 64-frame timeline used by the engine to separate immutable identity, slow-changing traits, contextual reactions, and the live state. It provides a fixed, easily navigable partition of frames into four layers: DnaCore (0), StableTraits (1), Context (2), and LiveState (3). The layer for any given frame index is obtained by dividing the index by 16 and casting the result to FrameLayer, ensuring quick, centralized categorization without scattered index arithmetic.

## Remarks
FrameLayer serves as an architectural boundary that clarifies how data should be treated across the frame lifecycle. Identity data stored in DnaCore is immutable after generation, while StableTraits drift very slowly, Context captures medium-term reactions, and LiveState is recomputed each turn. This separation reduces cross-cutting concerns and makes it simpler to apply appropriate update strategies, since components can reason about identity, trait drift, context, and live state in isolation before integrating results.

## Example
```csharp
int frameIndex = 37;
FrameLayer layer = (FrameLayer)(frameIndex / 16); // 37 / 16 == 2 -> FrameLayer.Context
```

## Notes
- Validate that the frame index is within 0..63; otherwise, (frameIndex / 16) may yield 4 or higher, which does not correspond to any FrameLayer value.


---
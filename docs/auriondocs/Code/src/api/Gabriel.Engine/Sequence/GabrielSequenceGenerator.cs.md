# GabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceGenerator : IGabrielSequenceGenerator
```


GabrielSequenceGenerator is a sealed class that builds a deterministic GabrielSequence by combining a seed-derived palette with a chosen visual primitive and orchestrating four layered animation passes. It exposes a single Generate method that accepts a seed, an optional ConversationState, and optional overrides for pattern or palette, and returns a 64-frame sequence distributed across four layers (DNA Core, Traits, Context, Live) with layer-specific timing, palette windows, and intensity. The resulting sequence is ready-to-use for personality-driven visuals without writing per-frame rendering logic yourself.

## Remarks
GabrielSequenceGenerator acts as the central orchestrator for Gabriel visuals, tying together PaletteTemplates, the PatternBundle, and Live-state modulation into a cohesive, repeatable asset. It encapsulates determinism (via the seed) while enabling runtime variation through state and optional overrides, avoiding ad-hoc assembly of frames in consumer code.

## Notes
- The four-layer rendering uses distinct palette windows and a per-layer phase shift to create a sense of progression while sharing a single palette and primitive.
- Live-state modulation reads ConversationState to adjust palette bounds and intensity per frame; if state is null, a default LiveStateProfile is used.
- Unknown paletteOverride or patternOverride values will fall back to seed-derived defaults rather than throwing.
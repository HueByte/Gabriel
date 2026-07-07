# GabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceGenerator : IGabrielSequenceGenerator
```


GabrielSequenceGenerator is a stateless heuristic engine that deterministically builds a GabrielSequence by selecting a color palette and a pattern from a seed, then rendering a 64-frame animation across four layered passes (DNA Core, Traits, Context, and Live). It accepts an optional patternOverride and paletteOverride; when overrides are provided and recognized, they influence the outcome, otherwise the seed governs the defaults via PaletteTemplates and SequenceCatalog. Each frame is produced with layer-specific timing, palette window, and intensity, and the Live layer adapts its palette window and intensity from ConversationState to reflect user state. The resulting GabrielSequence is accompanied by SequenceMetadata that records the seed, generation timestamp, and a concise summary of the chosen pattern and palette.

## Remarks
GabrielSequenceGenerator centralizes the construction of a personality-specific visual sequence by tying together palette selection, pattern choice, deterministic timing, and user state. It isolates rendering concerns from callers and guarantees reproducible results for a given seed and state, while still offering callers control via patternOverride and paletteOverride. The design supports deterministic experimentation: the same seed with identical state yields the same frames, while different seeds or overrides produce distinct identities.

## Notes
- Statelessness: The class has no mutable instance state, so it is safe to invoke concurrently from multiple threads.
- Override semantics: Unknown patternOverride or paletteOverride names gracefully fall back to seed-derived choices rather than throwing, ensuring predictable behavior for incomplete overrides.
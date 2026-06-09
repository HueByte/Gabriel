# GabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`  
> **Kind:** class

Generates a 64-frame visual sequence (GabrielSequence) from a numeric seed plus optional conversation state and explicit overrides. Reach for this when you need a repeatable, personality-driven animation that combines a seed-derived color palette and one of several animated pattern primitives; use overrides to pin the palette family or pattern primitive while retaining seed-based variation for per-pattern parameters.

## Remarks
GabrielSequenceGenerator synthesizes four layered time windows — DNA, Stable Traits, Context, and Live — that share a common palette and pattern primitive but differ in phase, palette window, and intensity to create visual depth and identity. The generator delegates palette selection/expansion and pattern parameterization to PaletteTemplates / SequenceCatalog / BuildPatternBundle, then produces per-frame images with Render. The Live layer is modulated by ConversationState (via LiveStateProfile) so the same seed can adapt to runtime context while preserving the underlying personality fingerprint.

## Example
```csharp
var generator = new GabrielSequenceGenerator();
long seed = 123456789;
ConversationState? state = null; // or some runtime state
// optional: pin to a known palette or pattern name
var sequence = generator.Generate(seed, state, patternOverride: null, paletteOverride: "aurora");
// sequence.Frames contains 64 rendered frames grouped into the four layers
```

## Notes
- The generator is deterministic with respect to the provided seed, pattern/palette overrides, and ConversationState; however, SequenceMetadata.GeneratedAt is set to DateTimeOffset.UtcNow so metadata will differ between runs even with identical inputs.
- If paletteOverride names are unknown, the code falls back to seed-derived palette selection (PaletteTemplates.PickByName returns null and Pick(seed) is used).
- Supplying patternOverride pins the primitive (e.g., plasma, waves) but per-pattern parameters remain derived from the seed — the override does not replace seed-based parameter fingerprinting.
- The class itself contains no mutable state; calls are reentrant. If you require byte-for-byte identical outputs including timestamps, post-process SequenceMetadata or set timestamps explicitly.
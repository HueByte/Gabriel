# GabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`  
> **Kind:** class

Generates a deterministic 64-frame GabrielSequence (four 16-frame layers: DNA, Traits, Context, Live) from a numeric seed, optional ConversationState and optional pattern/palette overrides. Use this when you need a reproducible visual personality — including palette, animated primitive, and live-state modulation — seeded from a fingerprint and optionally influenced by runtime conversation state or explicit overrides.

## Remarks
This sealed generator composes a visual "personality" by selecting a palette template and a pattern primitive, expanding that template into a 16-entry palette, and rendering four time-offset layers that share the same palette and pattern but differ in phase, palette window and intensity. Palette selection prefers an explicit override name but falls back to a seed-derived family when the override is unknown or null. The concrete per-pattern parameters are derived from a folded seed (XOR + cast to int) via a Random instance; pinning the pattern kind with patternOverride fixes the primitive but not the seed-fingerprinted parameters.

## Example
```csharp
var generator = new GabrielSequenceGenerator();
long seed = 1234567890L;
ConversationState? state = null; // or a real ConversationState instance
// Optional: pin a known pattern or palette by name (unknown names fall back to seed-derived)
var sequence = generator.Generate(seed, state, patternOverride: null, paletteOverride: "aurora");
// sequence.Frames contains 64 rendered frames; sequence.Metadata records seed and generation time
```

## Notes
- Determinism: for the same seed, pattern/palette override inputs and ConversationState, the generated frames are deterministic. However, GeneratedAt in metadata uses DateTimeOffset.UtcNow and will differ between runs.
- patternOverride pins the primitive BUT per-pattern numeric parameters (e.g., frequencies, offsets) remain derived from the seed; this allows the same primitive to look different across seeds.
- Unknown paletteOverride names are accepted but ignored (the implementation falls back to the seed-picked template). Validate names with PaletteTemplates if you need strict control.
- The parameter RNG is seeded by casting a folded 64-bit seed into a 32-bit int (unchecked). Very large or adversarially chosen seeds can collide after the cast, reducing entropy compared with a full 64-bit RNG seed.
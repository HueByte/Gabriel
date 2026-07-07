# GabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/GabrielSequenceGenerator.cs`  
> **Kind:** class

```csharp
public sealed class GabrielSequenceGenerator : IGabrielSequenceGenerator
```


GabrielSequenceGenerator is a sealed class that implements IGabrielSequenceGenerator and is responsible for generating a deterministic GabrielSequence by combining a chosen palette with a selected pattern, then evolving that identity across four layered time windows. Given a seed, optional patternOverride and paletteOverride values, it deterministically builds a 64-frame sequence (16 frames per layer across four layers: DNA Core, Traits, Context, and Live) where each layer shares the same base palette and pattern but applies distinct timing, palette ranges, and intensity. The paletteOverride takes precedence when provided; otherwise the seed drives the palette via PaletteTemplates.Pick. The Live layer additionally consults the ConversationState to modulate the palette window and intensity, giving the last layer a state-aware character. This class is what you reach for when you need reproducible, parameter-driven visual sequences that maintain a coherent identity across layers while still offering per-layer variation.

## Dependencies
- IGabrielSequenceGenerator
- Size
- ConversationState
- DateTimeOffset
- Random
- SequenceMetadata
- GabrielSequence
- PatternBundle

## Remarks
GabrielSequenceGenerator centralizes the process of mapping a numeric seed into a visually distinct GabrielSequence by layering four perspectives of the same pattern and palette. It encapsulates palette selection, pattern selection, and per-layer timing, ensuring a cohesive identity while still enabling variety through overrides and Live state modulation. It serves as the production mean for generating testable, deterministic sequences across sessions, personas and live states.

## Example
```csharp
var generator = new GabrielSequenceGenerator();
var seq = generator.Generate(
    seed: 123456789L,
    state: null,
    patternOverride: "plasma",
    paletteOverride: "heat"
);
```

## Notes
- The GeneratedAt timestamp is wall-clock time at generation, so results differ across runs even with the same seed.
- Pinning the primitive via patternOverride does not fully freeze per-pattern parameters; those remain fingerprinted to the seed.
- The Live layer uses ConversationState; if state is null, LiveStateProfile.From(null, Size) yields a default modulation.
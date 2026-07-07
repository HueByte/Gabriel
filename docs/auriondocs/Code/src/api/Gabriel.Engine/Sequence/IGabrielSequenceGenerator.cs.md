# IGabrielSequenceGenerator

> **File:** `src/api/Gabriel.Engine/Sequence/IGabrielSequenceGenerator.cs`  
> **Kind:** interface

```csharp
public interface IGabrielSequenceGenerator
```


Deterministically generates a full 64-frame Gabriel Sequence from a seed. Implementations provide a repeatable avatar animation derived from a stable identity, with optional live-state signals and user-specified skin overrides.

## Remarks
It sits at the boundary between identity and presentation: the seed establishes the baseline avatar, while state drives the latter frames (frames 48–63) to reflect mood, tempo, and engagement. The interface is stateless and safe to register as a singleton, enabling reuse across the application. PatternOverride and PaletteOverride, when provided, take precedence over seed-derived picks; if an identifier is unknown, the system falls back to seed-derived behavior rather than erroring (see SequenceCatalog). This decouples the caller from seed management and skin-picking logic, centralizing sequence generation in Gabriel engine.

## Example
```csharp
// Typical usage via DI
var generator = serviceProvider.GetRequiredService<IGabrielSequenceGenerator>();
long seed = /* personality's stable identity */
ConversationState? state = /* optional live-state signals */;
GabrielSequence sequence = generator.Generate(seed, state, patternOverride: "neon", paletteOverride: "aurora");
```

## Notes
- PatternOverride and PaletteOverride take precedence over seed-derived picks; unknown identifiers fall back to seed-derived behavior rather than erroring.
- Frames 48–63 are driven by ConversationState when provided, allowing mood/tempo/engagement signals to influence the latter portion of the sequence.
- The generator is stateless; it is safe to register as a singleton and call Generate without side effects.
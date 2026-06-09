# PatternKind

> **File:** `src/api/Gabriel.Engine/Sequence/PatternKind.cs`  
> **Kind:** enum

Defines the visual/animation primitive used by the pattern renderer. Use this enum to select one of the five built-in pattern grammars (Plasma, Waves, Spiral, Pulse, Shimmer) when configuring a project, conversation, or other owner of a generated sequence — if no explicit override is provided the rendering seed chooses one.

## Remarks
This enum is a lightweight discriminator that maps to the animation grammars implemented in Patterns.cs; each member represents a distinct visual style the generator can render. Typical callers either set an explicit override on the owning Project/Conversation to pin a look, or leave it unset and allow the seed-based selection to pick one automatically.

## Example
```csharp
// Pin a specific pattern on a project so it won't be chosen by the seed
project.Pattern = PatternKind.Spiral;

// Branch behavior based on the selected pattern
switch (project.Pattern)
{
    case PatternKind.Plasma:
        RenderPlasma();
        break;
    case PatternKind.Waves:
        RenderWaves();
        break;
    case PatternKind.Spiral:
        RenderSpiral();
        break;
    case PatternKind.Pulse:
        RenderPulse();
        break;
    case PatternKind.Shimmer:
        RenderShimmer();
        break;
}
```

## Notes
- Do not rely on the underlying numeric values of these enum members for long-term storage or interop; persist the name if stability is required.
- If an owning object does not set an explicit PatternKind, the generator's seed logic selects one — setting an override is the way to guarantee a particular look.
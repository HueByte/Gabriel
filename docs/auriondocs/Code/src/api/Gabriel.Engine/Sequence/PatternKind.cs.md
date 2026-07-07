# PatternKind

> **File:** `src/api/Gabriel.Engine/Sequence/PatternKind.cs`  
> **Kind:** enum

```csharp
public enum PatternKind
{
    Plasma,
    Waves,
    Spiral,
    Pulse,
    Shimmer,
}
```


PatternKind enumerates the five visual primitives that the generator can render. It serves as a type-safe selector used by the rendering pipeline to pick one of the predefined animation grammars defined in Patterns.cs. The seed logic selects a pattern when no override is set on the owning Project or Conversation, while explicit overrides let users pin a specific look without rerolling for it.

## Remarks

PatternKind decouples the choice of visual style from the concrete rendering implementation. It enables consumers to switch between Plasma, Waves, Spiral, Pulse, and Shimmer without referencing the underlying generation logic, which lives in Patterns.cs. By centralizing defaulting and overrides, it provides predictable visuals across components while still allowing project-specific customization.

## Notes

- Adding or renaming values in PatternKind must be kept in sync with the corresponding initialization and evaluation methods in Patterns.cs to avoid runtime mismatches.
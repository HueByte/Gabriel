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


PatternKind is an enumeration that lists the five primitive visual patterns the generator can render: Plasma, Waves, Spiral, Pulse, and Shimmer. Each member corresponds to a distinct animation grammar defined in Patterns.cs, enabling the rendering engine to switch between discrete pattern strategies at runtime. A seed determines which pattern is chosen by default when no explicit override is set on the owning Project or Conversation, while explicit overrides let a user pin a specific look without re-rolling for it.

## Remarks

PatternKind serves as a clean abstraction that decouples the rendering policies from the rest of the system. By centralizing the allowed visual primitives, the code that configures defaults or user overrides can reference a single type rather than multiple ad-hoc constants. It also makes it straightforward to extend visuals by adding new kinds in Patterns.cs and PatternKind, with minimal ripple effects across callers.

## Notes

- If you extend PatternKind with new values, ensure the dispatch logic covers the new cases to avoid unexpected fallbacks.
- When changing the seed/default mapping, update the corresponding documentation and tests to reflect the new default visual.
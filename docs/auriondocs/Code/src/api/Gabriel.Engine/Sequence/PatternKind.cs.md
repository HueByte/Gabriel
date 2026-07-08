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


PatternKind enumerates the five pattern primitives the generator can render. Each member corresponds to a distinct animation grammar defined in Patterns.cs: Plasma, Waves, Spiral, Pulse, and Shimmer. By default, the seed selects one when no explicit override is provided on the owning Project or Conversation; explicit overrides let you pin a specific look without rerolling for it.

## Remarks
PatternKind acts as a stable, high-level selector that decouples the rendering pipeline from the concrete pattern implementations. It defines the allowed set of looks and provides a single knob for consumers to request a style. The actual rendering logic and per-pattern parameters live in the Patterns.* implementations; PatternKind simply selects which primitive to use, allowing the system to evolve patterns independently and new patterns to be added with minimal impact to callers.

## Example
```csharp
PatternKind kind = PatternKind.Plasma;
```

## Notes
- Do not rename or reorder the enum values, as existing overrides and serialized data rely on these exact identifiers.
- When introducing a new pattern, extend both PatternKind and the corresponding PatternInit/Renderer in Patterns.cs to maintain parity and preserve backward compatibility.
# PatternKind

> **File:** `src/api/Gabriel.Engine/Sequence/PatternKind.cs`  
> **Kind:** enum

Represents the five primitive visual patterns the renderer can produce. Use this enum to select or pin a specific visual motif for a Project or Conversation; when no explicit choice is provided the generator's seed picks one automatically.

## Remarks
Each member corresponds to a distinct animation grammar (see Patterns.cs) implemented by the rendering/generation pipeline. The seed-based selection yields a consistent look when no override is set on the owning Project or Conversation; conversely, setting an explicit PatternKind forces that visual style so the appearance remains stable across re-runs.

## Notes
- These enum values may be persisted or serialized by callers; avoid reordering or renumbering existing members to preserve compatibility.
- Adding new values can change the distribution of seed-chosen patterns; expect different seeds to map to different visuals after extension.
- Consumers should map each enum member to the corresponding implementation in Patterns.cs rather than relying on string/int names directly.
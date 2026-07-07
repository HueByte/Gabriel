# MemoryEntry

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntry.cs`  
> **Kind:** class

```csharp
public class MemoryEntry
```


Represents a single remembered item (a "memory") that Gabriel stores for a user either globally (user-scope) or scoped to a specific project. Use MemoryEntry.Create to construct a new memory and MemoryEntry.Update to modify an existing memory; entries are persisted in the application's database and intended to be surfaced by the agent's memory tools and the UI editor.

## Remarks
MemoryEntry is a small persistence-backed DTO that captures the agent-facing shape for saved memories: a scoped identifier (UserId and optional ProjectId), a kebab-case Name used by agent tools and links, a short Description for relevance filtering, and a Body containing the memory text. The design keeps CreatedAt immutable (set at creation) while UpdatedAt is bumped on Update so the UI can show when the item was first remembered versus when it was last changed. Validation (non-empty Name/Description/Body and non-empty UserId at creation) is performed by the type itself; uniqueness of Name within a (UserId, ProjectId) scope is a conceptual constraint mentioned in the comments and is expected to be enforced at a higher layer (repository/DB schema).

## Notes
- Create throws ArgumentException if userId is Guid.Empty; calls to create must provide a valid user id.
- Name/Description/Body are validated for non-empty/whitespace; callers must supply trimmed, meaningful values.
- Update is idempotent for the same Name in the same scope: it replaces Type/Description/Body and updates UpdatedAt while leaving CreatedAt unchanged.
- The class does not enforce kebab-case formatting or scope-unique constraints itself; those conventions/constraints are expected to be handled by tools, prompts, or persistence rules outside this type.
- Instances are mutable only through the provided factory and Update method; there is no internal synchronization (not thread-safe).
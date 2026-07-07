# MemoryEntry

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntry.cs`  
> **Kind:** class

```csharp
public class MemoryEntry
```


Represents a single remembered item (a "memory") owned by a user and optionally scoped to a specific project. Use MemoryEntry.Create to construct new entries and MemoryEntry.Update to modify an existing entry; entries are stored in the database and carry metadata (Id, CreatedAt, UpdatedAt) so the UI and agent can list, display, and update them.

## Remarks
MemoryEntry is a small, persisted DTO that models one unit of the app's automatic memory system. ProjectId == null indicates a user-global memory (applies across all conversations for that user); a non-null ProjectId narrows the memory to a specific project. The class enforces minimal invariants (non-empty name/description/body, non-empty user id) and keeps setters private so instances are created via the Create factory or updated through the Update method. Timestamps are managed here: CreatedAt is set once on creation and never changed by Update, while UpdatedAt is refreshed on Update.

## Example
```csharp
// Create a new user-scoped memory
var memory = MemoryEntry.Create(
    userId: userId,
    projectId: null, // user-global
    type: MemoryEntryType.Feedback,
    name: "prefer-short-methods",
    description: "Prefer shorter methods for readability",
    body: "Keep methods under ~50 lines.\n\n**Why:** Easier to reason about.\n**How to apply:** Split long methods into well-named helpers.");

// Later, update the same memory's type or content
memory.Update(
    type: MemoryEntryType.ProjectRule,
    description: "Prefer short methods; add examples",
    body: "Keep methods short; when in doubt extract helper methods.\n\n**Why:** ...");
```

## Notes
- Create throws an ArgumentException if userId is Guid.Empty or if name/description/body are null/empty/whitespace.
- Name, Description, and Body are trimmed on creation and update; the class does not validate kebab-case or enforce uniqueness of Name within scope — uniqueness should be enforced at a higher level (database unique constraint or repository logic).
- CreatedAt is set when the instance is created and is intentionally not modified by Update; UpdatedAt is set to UtcNow on Update.
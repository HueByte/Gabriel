# MemoryEntry

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntry.cs`  
> **Kind:** class

Represents a single "memory" that Gabriel can recall. Use this entity when creating, persisting or updating remembered items that are scoped either to a user (ProjectId == null) or to a specific project (ProjectId set). Prefer the static Create factory to construct instances and the Update method to change an existing entry so validation and timestamp semantics are applied consistently.

## Remarks
This class mirrors the auto-memory file shape used by Claude-style agents: a short kebab-case name (slug), a one-line description for relevance checks, the body with the content, and a type that tells the model how to treat the entry. Memories are stored in the application's database (EF/SQLite) rather than as files so the UI can offer atomic CRUD operations; the class encodes scoping (UserId always set, ProjectId nullable) so memories never leak across users and can optionally be limited to a single project.

## Example
```csharp
// Create a new user-scoped memory
var memory = MemoryEntry.Create(
    userId: currentUserId,
    projectId: null, // user-scope
    type: MemoryEntryType.Fact,
    name: "preferred-code-style",
    description: "Project-wide code style preferences",
    body: "Use tabs for indentation. **Why:** consistency with legacy code. **How to apply:** configure editor"
);

// Persist via repository/EF and later update in-place
repository.Add(memory);
repository.SaveChanges();

// Later, update the same entry (preserves CreatedAt, bumps UpdatedAt)
memory.Update(MemoryEntryType.Fact, "Updated description", "Updated body");
repository.Update(memory);
repository.SaveChanges();
```

## Notes
- The constructor is private — use MemoryEntry.Create(...) to get a valid instance.
- Create and Update validate that name, description and body are non-empty; they throw ArgumentException for whitespace or empty values.
- Name is trimmed but not checked for kebab-case or uniqueness here; the code comments expect the slug to be unique within (UserId, ProjectId) and kebab-case, but enforcement/constraints are the responsibility of the storage layer or higher-level services.
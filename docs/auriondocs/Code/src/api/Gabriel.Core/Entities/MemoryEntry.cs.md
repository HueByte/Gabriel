# MemoryEntry

> **File:** `src/api/Gabriel.Core/Entities/MemoryEntry.cs`  
> **Kind:** class

Represents a single piece of remembered information (a "memory") owned by a user and optionally scoped to a specific project. Use this type when creating, persisting or updating agent memories through the application's data layer (EF/SQLite) and memory tools rather than storing them as plaintext files.

## Remarks
MemoryEntry mirrors the auto-memory shape used by Claude-like agents: a short kebab-case name (slug), a one-line description for relevance scoring, the full body text, and a type that indicates how the agent should interpret the memory. Instances are stored in the database to support atomic CRUD operations and UI editing. The class enforces basic presence validation and trims inputs; scope and uniqueness (Name unique within the (UserId, ProjectId) pair) are conventions the surrounding system and/or database constraints must uphold. The private constructor plus the static Create factory ensure valid instances (UserId required); Update is idempotent and only refreshes UpdatedAt so CreatedAt preserves the original creation time.

## Example
```csharp
// Create a user-scoped memory (applies to all projects for the user)
var memory = MemoryEntry.Create(
    userId: userId,
    projectId: null,
    type: MemoryEntryType.Fact,
    name: "daily-backup-policy",
    description: "When to run backups and retention policy.",
    body: "Run full backups every Sunday. Why: ensure recoverability. How: use nightly script and keep 30 days.");

// Later, update the same entry's type/description/body (keeps CreatedAt)
memory.Update(
    type: MemoryEntryType.Rule,
    description: "Schedule and retention clarified.",
    body: "Run full backups every Sunday at 02:00 UTC. Why: minimize load. How: run backup.sh and rotate logs.");
```

## Notes
- Create throws ArgumentException if userId is Guid.Empty; name/description/body must be non-empty or an ArgumentException is thrown.
- The class trims name/description/body on create/update but does not validate kebab-case formatting — that convention is enforced elsewhere.
- Name uniqueness within (UserId, ProjectId) is a system-level constraint (not enforced by this type); the agent's memory_save tool treats Update as the idempotent path when a name collision is detected.
- CreatedAt is set when the instance is created and is not modified by Update; UpdatedAt is refreshed on Update.
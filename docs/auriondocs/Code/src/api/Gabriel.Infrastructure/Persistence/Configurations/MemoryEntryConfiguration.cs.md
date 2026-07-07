# MemoryEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MemoryEntryConfiguration : IEntityTypeConfiguration<MemoryEntry>
```


MemoryEntryConfiguration configures EF Core mapping for MemoryEntry to the MemoryEntries table. It defines the primary key, required fields, maximum lengths, and two indexes that optimize common access patterns: a hot path for listing memories by user and scope (UserId, ProjectId, UpdatedAt) and a uniqueness constraint on (UserId, ProjectId, Name) to support slug usage by the memory_save tool. The configuration mirrors the domain’s need to distinguish between project-scoped memories and user-scoped memories and to enforce data integrity around upserts and lookups.

## Remarks
This class decouples persistence concerns from the MemoryEntry domain model by centralizing database schema details (table name, keys, constraints, and indexes) in one place. It encodes the business rules around memory scoping and slug usage: memories are identified by Id, but their semantic slug (Name) must be unique within each (UserId, ProjectId) scope, while still supporting both project-scoped and user-scoped memories. The two hot lookup paths reflect the most common query patterns the application performs when managing memories.

## Notes
- The (UserId, ProjectId, Name) unique index relies on how NULLs are treated by the underlying database to differentiate user-scoped memories (ProjectId = NULL) from project-scoped ones. This behavior is database-specific (notably SQLite’s handling of NULLs in unique constraints) and should be considered if you switch databases. 
- If you rename properties, tables, or adjust nullability, update this configuration accordingly to preserve the intended schema and query performance.
- Changes to indexing can impact performance of the hot path queries (listing by UpdatedAt) and upsert semantics; validate performance after any modification.
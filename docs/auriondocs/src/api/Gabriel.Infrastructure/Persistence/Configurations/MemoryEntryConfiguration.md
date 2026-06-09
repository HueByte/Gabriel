# MemoryEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs`  
> **Kind:** class

Configures the EF Core mapping for the MemoryEntry entity: sets the table name, primary key, property constraints (requiredness and max lengths), and the commonly used indexes including a composite index for listing a user's memories by scope and UpdatedAt and a unique composite index that enforces slug (Name) uniqueness within a (UserId, ProjectId) scope.

## Remarks
This class centralizes persistence rules for MemoryEntry so the DbContext can apply a single source of truth for schema and indexes. It marks fields that must be present (UserId, Type, Name, Description, Body, CreatedAt, UpdatedAt), limits Name and Description lengths, and defines two "hot" lookup indexes used by read and upsert paths: a non-unique index (UserId, ProjectId, UpdatedAt) to efficiently list a user's memories in a scope ordered by UpdatedAt, and a unique index (UserId, ProjectId, Name) used by the memory_save upsert logic. The configuration assumes ProjectId is nullable to represent user-scoped memories.

## Example
```csharp
// In your DbContext implementation
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new MemoryEntryConfiguration());
}
```

## Notes
- The uniqueness of (UserId, ProjectId, Name) depends on how the database provider treats NULLs. The project relies on SQLite's default behaviour where NULLs are treated as distinct so a user-scoped memory (ProjectId == null) can share a Name with a project-scoped memory. Verify semantics for other providers or use a migration/workaround if your target DB handles NULLs differently.
- UpdatedAt is included in an index to support ordering; if UpdatedAt is updated frequently this may increase index maintenance cost.
- Name and Description length limits are enforced at the EF/model level and will be reflected in migrations—ensure client code respects these limits to avoid truncation or validation errors.

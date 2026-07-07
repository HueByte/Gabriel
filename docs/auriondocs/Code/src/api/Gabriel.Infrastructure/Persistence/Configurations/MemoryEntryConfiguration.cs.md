# MemoryEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MemoryEntryConfiguration : IEntityTypeConfiguration<MemoryEntry>
```


Configures the EF Core mapping for MemoryEntry. It binds MemoryEntry to the MemoryEntries table, marks the primary key and required properties, enforces length constraints on string fields, and defines two targeted indexes to support common access patterns: a hot-path index for listing memories by UserId within a given scope (ordered by UpdatedAt), and a composite unique index on (UserId, ProjectId, Name) to enforce slug uniqueness within a scope (using Name as the slug key).

## Remarks
EF Core configuration here encapsulates persistence concerns, keeping the MemoryEntry domain model separate from database specifics while aligning the database schema with typical query patterns. Making ProjectId nullable allows MemoryEntry to exist in either a user-wide namespace or a specific project, and the two indexes encode the intended access paths and slug semantics used by the memory_save workflow. This configuration serves as the single source of truth for how MemoryEntry is stored and queried at the persistence layer.

## Example
```csharp
// Typical usage inside your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new MemoryEntryConfiguration());
}
```

## Notes
- The unique index (UserId, ProjectId, Name) enforces per-scope slug uniqueness; due to ProjectId being nullable, memories in user scope (ProjectId = NULL) are allowed to share slugs with project-scoped memories as described by the underlying provider's NULL handling semantics. If migrating to a provider with different NULL semantics, verify the constraint behavior.
- Ensure the configuration is applied during model creation (e.g., via OnModelCreating) so EF Core creates the intended schema and indices.
- The Name and Description constraints (max lengths) help maintain consistent data sizing and prevent overly long entries.
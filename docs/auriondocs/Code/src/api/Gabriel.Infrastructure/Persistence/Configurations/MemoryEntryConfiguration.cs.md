# MemoryEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MemoryEntryConfiguration : IEntityTypeConfiguration<MemoryEntry>
```


Configures the MemoryEntry entity for EF Core. This fluent configuration maps MemoryEntry to the MemoryEntries table, declares Id as the primary key, and enforces required properties and max lengths, plus two indices for common queries: a hot lookup path index on (UserId, ProjectId, UpdatedAt) and a unique index on (UserId, ProjectId, Name) to enforce slug uniqueness per user/project.

## Remarks
By isolating persistence mapping here, MemoryEntry remains a plain data model while the database schema is defined in this single place. The two indices reflect typical access patterns: fast listing of memories within a user scope by UpdatedAt, and strict uniqueness of slugs within (UserId, ProjectId). This separation also makes it easier to adapt the data layer without altering the entity.

## Notes
- In SQLite, the unique index on (UserId, ProjectId, Name) treats NULL ProjectId values as distinct; this enables a slug to be reused across user-scope and project-scope memories but also means identical (UserId, Name) pairs with NULL ProjectId won't violate the constraint.
- Ensure this configuration is registered in the DbContext to enforce the mappings.
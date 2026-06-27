# MemoryEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MemoryEntryConfiguration.cs`  
> **Kind:** class

Configures the Entity Framework Core mapping for MemoryEntry: maps the entity to the MemoryEntries table, declares the primary key, sets which properties are required and their maximum lengths, and creates the indexes used by common lookup patterns (listing by UpdatedAt and enforcing per-scope slug uniqueness). Register this configuration with your DbContext (via ApplyConfiguration or ApplyConfigurationsFromAssembly) so the model and generated migrations include these constraints and indexes.

## Remarks
This class centralizes schema-related decisions for MemoryEntry so the shape of the database (table name, nullability, column lengths and indexes) is kept close to the entity definition. It specifically provides a composite index (UserId, ProjectId, UpdatedAt) optimized for listing a user's memories in a scope ordered by UpdatedAt, and a composite unique index (UserId, ProjectId, Name) that enforces the intended slug/Name uniqueness within a (user, project) scope. The ProjectId property is left nullable to represent user-scoped memories; the uniqueness semantics for NULL ProjectId can differ between database providers, so the index was chosen to express the desired logical scope rather than provider-specific NULL behaviour.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply single configuration
    modelBuilder.ApplyConfiguration(new MemoryEntryConfiguration());

    // — or — apply all configurations in the assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(MemoryEntryConfiguration).Assembly);
}
```

## Notes
- The composite unique index uses (UserId, ProjectId, Name). NULL handling for ProjectId can vary by provider (SQLite treats NULLs specially); verify provider behaviour if you rely on exact NULL semantics for uniqueness.
- The (UserId, ProjectId, UpdatedAt) index is ordered to support queries that filter by UserId and ProjectId and sort by UpdatedAt. If queries omit the leading columns, the index may not be used for ordering.
- Changing max lengths or requiredness will affect migrations and the resulting schema; ensure consuming code respects the declared max lengths (Name 128, Description 512) to avoid DB exceptions.
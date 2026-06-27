# ProjectConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`  
> **Kind:** class

Configures the EF Core mapping for the Project aggregate: table name, keys, property constraints (requiredness and lengths), indexes (including a filtered index for the per-user default project), the files navigation backed by a private field, and the cascade delete relationship to project files. Use this configuration when building the DbContext model so the database schema and query-related indexes reflect the domain invariants and performance assumptions.

## Remarks
This class centralizes persistence concerns for Project so the domain shape, database schema and query patterns stay consistent across the application. It encodes domain-level invariants (required properties, max lengths), adds indexes for the common access patterns (user’s projects sorted by UpdatedAt and quick lookup of a user’s default project), and maps the Files navigation to a private backing field to preserve aggregate encapsulation.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new ProjectConfiguration());
}
```

## Notes
- The filtered index uses an explicit SQL filter ("IsDefault" = 1); quoting and filter syntax are provider-specific — confirm the target database supports filtered indexes and the exact filter expression when generating migrations.
- The Files navigation is mapped to a private field named _files; renaming that field in the Project class requires updating this configuration or migrations may break.
- Deleting a Project will cascade-delete its Files because of DeleteBehavior.Cascade; ensure callers expect this side effect.

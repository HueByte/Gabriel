# ProjectConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`  
> **Kind:** class

Configures the Entity Framework Core mapping for the Project aggregate: table name, primary key, column constraints (lengths and required flags), indexes (including a filtered index for the per-user default project), navigation/backing-field mapping for the Files collection, and the relationship between Project and its file entities with cascade delete. Use this configuration during model building (e.g. in DbContext.OnModelCreating) to ensure the database schema and EF model match the domain's invariants.

## Remarks
This class centralizes persistence concerns for the Project entity so the domain type stays focused on business rules. It enforces storage-level constraints (column lengths, nullability), creates efficient lookup indexes used by the application (owner + UpdatedAt for listing, owner + IsDefault filtered index for identity-style default lookup), and preserves aggregate encapsulation by mapping the Files navigation to a private backing field. Keeping these mappings in a dedicated IEntityTypeConfiguration keeps DbContext configuration concise and makes migrations deterministic.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new ProjectConfiguration());
}
```

## Notes
- The filtered index uses HasFilter("\"IsDefault\" = 1"); the literal filter expression is provider-specific and may require adjustment or different quoting for some database engines—verify generated migrations for your provider.
- The configuration maps the Files navigation to a private field named _files and sets PropertyAccessMode.Field; the Project type must expose a navigation property named Files and contain a backing field with that exact name for this mapping to work.
- Cascade delete is enabled for the Project -> Files relationship; deleting a Project will delete its files at the database level. Ensure this matches the desired lifecycle semantics before applying migrations.

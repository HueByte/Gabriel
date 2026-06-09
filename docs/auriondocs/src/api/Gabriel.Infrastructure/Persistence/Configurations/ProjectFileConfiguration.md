# ProjectFileConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs`  
> **Kind:** class

Configures how the ProjectFile entity is mapped to the database for Entity Framework Core: table name, primary key, required properties, string length limits, and indexes used for common queries and uniqueness enforcement. Use this configuration when building the EF Core model (e.g., in DbContext.OnModelCreating) so the database schema and EF behavior match the application's expectations.

## Remarks
This class implements `IEntityTypeConfiguration<ProjectFile>` to centralize persistence rules for the ProjectFile entity. It ensures schema-level constraints (required columns and max lengths) and adds two indexes: one to support listing a project's files ordered by upload time, and a unique composite index on (ProjectId, RelativePath) to prevent duplicate file paths across a project even when files are renamed. The code comments indicate that same-name collisions within a project are rejected at the service layer while the unique index prevents cross-rename races or duplicates.

## Example
```csharp
// In your DbContext implementation
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new ProjectFileConfiguration());
}
```

## Notes
- The uniqueness guarantee comes from a unique index on (ProjectId, RelativePath); the service layer additionally enforces same-name rejection within a project as indicated in the source comment.
- Changing max lengths or required flags here requires an EF Core migration to update the database schema.
- The index on (ProjectId, UploadedAt) is non-unique and intended for efficient ordered queries (e.g., listing recent uploads).
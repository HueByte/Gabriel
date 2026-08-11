# ProjectFileConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
```


Configures how the ProjectFile entity is mapped to the database in EF Core: it specifies the table name, primary key, property constraints, and critical indexes. Use this configuration when setting up the persistence model to ensure the schema enforces invariants like per-project RelativePath uniqueness and efficient retrieval of a project's files by upload time.

## Remarks
By centralizing the persistence mapping, this configuration keeps storage concerns decoupled from the domain model while enforcing schema-level invariants. The two indexes express intended query patterns: a composite index on (ProjectId, UploadedAt) for listing a project's files in upload order, and a uniqueness index on (ProjectId, RelativePath) to prevent duplicate paths within a single project (complementing service-layer validation).

## Notes
- The configuration marks the following fields as required and constrains their lengths: Name (max 256), RelativePath (max 512), ContentType (max 128), SizeBytes, and UploadedAt. This aligns the database schema with the domain expectations for a ProjectFile.
- The per-project RelativePath uniqueness constraint ensures cross-renaming scenarios are handled deterministically by the database in tandem with the service-layer checks described in the code comments.
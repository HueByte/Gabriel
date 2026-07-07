# ProjectFileConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
```


This EF Core configuration class wires the ProjectFile entity to the database by mapping it to the ProjectFiles table, applying required constraints, and establishing indexes that support common access patterns. It declares Id as the primary key and marks ProjectId, Name, RelativePath, SizeBytes, ContentType, and UploadedAt as required, while constraining string lengths for Name (256), RelativePath (512), and ContentType (128).

Two composite indexes are defined: a non-unique index on (ProjectId, UploadedAt) to optimize listing a project's files in order of upload time, and a unique index on (ProjectId, RelativePath) to prevent duplicates of the same file name within a single project and to support stable behavior when files are renamed in the UI. The inclusion of the Unique index aligns with the documented rule that same-name within a project is rejected by the service layer, while the RelativePath uniqueness ensures cross-rename scenarios remain deterministic.

## Remarks
This class centralizes the persistence-facing configuration for ProjectFile, keeping DbContext lean and reflecting domain invariants at the database level. By encapsulating the mapping and constraints here, the design promotes separation of concerns: domain logic lives in services, while the EF Core model layer enforces structural rules and performance characteristics at rest. The two indexes express the primary read patterns: fast per-project access by upload time, and strict per-project name uniqueness to prevent conflicting entries.

## Notes
- The unique composite index on (ProjectId, RelativePath) enforces per-project RelativePath uniqueness; attempts to insert or rename to a duplicate path within the same project will fail at the database level.
- The non-unique composite index on (ProjectId, UploadedAt) is intended to optimize queries that list a project's files sorted by upload time; ensure query paths leverage this pattern to benefit from the index.
- If the ProjectFile shape changes (properties or names), this configuration must be updated accordingly to preserve constraints and indexes.
# ProjectFileConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
```


Configures the EF Core mapping for the ProjectFile entity. It maps to the ProjectFiles table, enforces a primary key on Id, and applies required-field constraints and length limits to model properties such as ProjectId, Name, RelativePath, SizeBytes, ContentType, and UploadedAt. It also defines two indices: a non-unique index on (ProjectId, UploadedAt) to support per-project file listings ordered by upload time, and a unique index on (ProjectId, RelativePath) to prevent naming collisions within the same project. This configuration, together with service-layer validation, ensures data integrity for project-scoped files and supports reliable rename handling by keeping per-project paths unique.

## Remarks

By separating EF Core configuration from the entity and domain logic, this class centralizes persistence concerns and makes schema decisions explicit in one place. It clarifies which constraints exist at the database level and how they align with the higher-level domain rules for file naming within a project.

## Notes

- The per-project uniqueness is enforced at the database level via the (ProjectId, RelativePath) unique index; ensure the service layer complements this with user-friendly checks when creating or renaming files.
- The (ProjectId, UploadedAt) index is for query performance and is not a uniqueness constraint; when querying, explicitly order by UploadedAt to rely on the intended ordering.
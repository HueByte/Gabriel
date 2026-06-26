# ProjectFileConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectFileConfiguration.cs`  
> **Kind:** class

Configures the Entity Framework Core mapping for the ProjectFile entity: table name, primary key, required properties, length limits, and indexes used for listing and uniqueness enforcement. This configuration is applied during model building and is the canonical place to express database-level constraints and query-supporting indexes for project files.

## Remarks
Centralizes database schema concerns for ProjectFile so the domain/service code can rely on database-enforced constraints (required columns, maximum lengths) and indexes optimized for common queries. The non-unique index on (ProjectId, UploadedAt) supports efficient listing of a project's files ordered by upload time; the unique index on (ProjectId, RelativePath) enforces that a given relative path exists at most once per project and complements service-layer checks to keep file-name/r ename semantics consistent.

## Notes
- The service layer rejects same-name files within a project, but concurrent requests may still race; the unique index provides final enforcement. Handle database unique-constraint violations (e.g. DbUpdateException) and translate them to a user-friendly error.
- Changing the declared max lengths (Name, RelativePath, ContentType) requires a schema migration; existing data exceeding new limits will cause migration or runtime errors if not handled.
- The index on UploadedAt does not specify sort direction — queries should explicitly order by UploadedAt when deterministic ordering is required.
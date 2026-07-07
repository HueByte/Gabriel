# ProjectConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
```


Configures the EF Core mapping for the Project entity using the Fluent API. It maps Project to the Projects table, applies key and property constraints (e.g., required OwnerUserId and Name with a 128-character limit), wires up the one-to-many relationship with Files, and defines indexes to support common queries and invariants (a user’s projects ordered by UpdatedAt, and a per-user default with a filtered index). It also maps the private _files backing field for the Files navigation to ensure mutation flows through the aggregate, with cascade deletes for related files.

## Remarks
These lines centralize persistence concerns for the Project type, keeping domain entities free of EF Core specifics. They guarantee that the database schema reflects the entity invariants (required fields and length limits) and optimize common access patterns through targeted indexes, such as listing a user’s projects by UpdatedAt and enforcing at most one default project per user via a filtered index. It also demonstrates mutating the Files collection through the aggregate by using a private backing field mapped to the navigation property.

## Notes
- Filtered index requires provider support; the HasFilter clause relies on database-specific SQL. Ensure your database provider supports filtered indexes before migrations.
- Backing field mapping: the Files navigation uses a private field (_files) and Field access. Ensure the field exists on Project and is properly initialized to avoid null-reference issues when mutating the collection.
- Length constraints and required flags: Changing MaxLength or IsRequired values may require migrations and could affect existing data.
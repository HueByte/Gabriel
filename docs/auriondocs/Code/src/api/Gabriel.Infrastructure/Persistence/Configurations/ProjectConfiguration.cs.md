# ProjectConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
```


ProjectConfiguration is the EF Core configuration for the Project aggregate. It maps the Project entity to the Projects table, enforces key and property constraints, and defines how the Files navigation and related entities are persisted. It also introduces two indexes to support common access patterns: a user-owned list ordered by UpdatedAt, and a filtered index that enforces at most one Default per user. It uses a private backing field for the Files collection to allow mutation through the aggregate while keeping the navigation encapsulated, and it enables cascade delete for related File entities.

## Remarks
By isolating persistence mapping in a dedicated configuration class, domain types remain free of persistence concerns, improving separation of concerns and testability. The filtered index on IsDefault optimizes the common invariant of "at most one Default per user"; note that support for filtered indexes depends on the database provider, so migrations should be reviewed when changing providers (SQLite explicitly supports such indexes, but behavior may vary elsewhere). Mapping the private _files field enables encapsulated mutation of the Files collection while still letting EF Core materialize the relationship when needed.

## Notes
- Filtered index support is provider-specific; ensure your database provider supports the HasFilter clause.
- Mapping the private field _files requires the domain type to declare this backing field consistent with the configuration.
- Cascade delete means deleting a Project will also remove its related File entities; verify this aligns with domain invariants to prevent unintended data loss.

## Dependencies
- IEntityTypeConfiguration
- Project
- UpdatedAt
- Metadata
- PropertyAccessMode
- DeleteBehavior

## Symbol To Document
- Name: ProjectConfiguration
- Kind: class
- File: src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs
- Language: csharp
- ID: b02ebe11-7f32-41d2-a64f-030d65259805
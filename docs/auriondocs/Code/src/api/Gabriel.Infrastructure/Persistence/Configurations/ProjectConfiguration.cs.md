# ProjectConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/ProjectConfiguration.cs`  
> **Kind:** class

```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
```


Configures the EF Core mapping for the Project aggregate. It specifies the target database table, the primary key, required properties and maximum lengths, and the two indices used to support common queries. It also wires the one-to-many relationship to ProjectFile with cascade delete and configures the Files navigation to be field-backed for encapsulation.

## Remarks
EF Core model configuration like this centralizes persistence concerns for the Project aggregate, keeping domain logic decoupled from database specifics. It defines a compact index strategy (a per-user-by-updated ordering index and a filtered index ensuring at most one Default per user) to support efficient queries while preserving invariants. The use of a private backing field (_files) for the Files collection demonstrates a deliberate boundary between the domain model and EF Core's change-tracking, enabling controlled mutation of the aggregate's children.

## Notes
- Filtered index (HasFilter) relies on database provider support for partial indexes (SQLite supports this). Other providers may ignore the filter or require different syntax.
- Field-backed navigation means callers mutate the Files collection through the aggregate’s domain methods (or through the aggregate’s intended mutation pathways) to maintain invariants; direct field access is managed by EF Core.

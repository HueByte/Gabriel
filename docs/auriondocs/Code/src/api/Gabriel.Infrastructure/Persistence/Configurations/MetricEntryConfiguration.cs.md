# MetricEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MetricEntryConfiguration : IEntityTypeConfiguration<MetricEntry>
```


MetricEntryConfiguration configures how MetricEntry persists to the database. It maps to the MetricEntries table, enforces property constraints (Id as key, System length cap, Metric as required JSON payload, and CreatedAt as required), and defines indexes to support the application's common read patterns.

## Remarks
By centralizing the EF Core mapping here, persistence concerns are kept separate from the domain entity, making it straightforward to adjust storage details (such as length limits or indexing) without touching the domain model. The composite index on System and CreatedAt enables fast retrieval of recent entries for a specific system and for systems sharing a prefix, while a separate CreatedAt index supports cleanup queries that delete older data.

## Notes
- The 128-character limit on System bounds index size and enforces a sane naming convention.
- Metric is stored as JSON text (TEXT in SQLite); while some databases offer a native JSON type, this configuration remains valid across stores, and JSON-related queries may be performed when supported.
- The index on (System, CreatedAt) is designed to support both per-system recent-entry queries and broader recent-entry scans; if you alter storage or query patterns, revisit the indexing strategy accordingly.
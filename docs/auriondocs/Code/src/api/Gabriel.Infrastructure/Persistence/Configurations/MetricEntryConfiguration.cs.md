# MetricEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MetricEntryConfiguration : IEntityTypeConfiguration<MetricEntry>
```


Defines how MetricEntry is persisted in EF Core: maps to the MetricEntries table, configures the primary key and field constraints, and establishes indexes to support time-based lookups. This configuration centralizes schema decisions so higher-level data access doesn't need to repeat table names, constraints, or index hints, and it enables efficient retrieval of recent metrics per system or by system prefix.

## Remarks
Serves as the boundary between the MetricEntry domain model and the database schema, encapsulating table naming, key definition, and data constraints along with performance-oriented indexes. The composite index on System and CreatedAt, plus the separate CreatedAt index, encode the two primary access patterns: recent metrics for a specific system and recent metrics for systems sharing a prefix. The Metric property stores JSON payloads as TEXT (SQLite) to accommodate variable content; this choice is deliberately tailored to the storage engine and may differ with other providers.

## Notes
- The 128-character limit on System is intentional to bound index size; callers should not rely on longer subsystem names.
- Metric is a JSON payload; in SQLite it is stored as TEXT; if you switch to a database with native JSON types, mapping and storage semantics may differ.
- The index order (System, CreatedAt) is chosen to support the two primary read patterns; ensure queries use the same column order to leverage the index. In non-SQLite databases, verify that DESC handling remains efficient for CreatedAt.
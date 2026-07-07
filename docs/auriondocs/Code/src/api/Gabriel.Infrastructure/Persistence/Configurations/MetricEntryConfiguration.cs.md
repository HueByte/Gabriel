# MetricEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs`  
> **Kind:** class

```csharp
public class MetricEntryConfiguration : IEntityTypeConfiguration<MetricEntry>
```


MetricEntryConfiguration defines how EF Core maps the MetricEntry entity to the database. It configures the table name MetricEntries, declares Id as the primary key, and enforces a 128-character maximum on System, while requiring the Metric JSON payload and CreatedAt timestamp. It also registers two indexes: a composite index on (System, CreatedAt) to support the common read patterns of retrieving recent metrics per system or by system prefix, and a standalone index on CreatedAt to enable efficient cleanup of old data. The Metric field stores JSON payloads as TEXT (SQLite does not have a native JSON type), enabling flexible payloads and optional SQL-side JSON queries via json_extract/json_each if needed.
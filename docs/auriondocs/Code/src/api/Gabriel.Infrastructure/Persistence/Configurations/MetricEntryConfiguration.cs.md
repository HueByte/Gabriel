# MetricEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs`  
> **Kind:** class

Configures the EF Core mapping for MetricEntry entities: sets the table name, primary key, required properties, length constraint for the System column, stores Metric as JSON-capable text, and creates the indexes used by the common read and cleanup queries. Add this configuration to your DbContext model builder so the database schema and indexes match the application's read patterns and storage expectations.

## Remarks
This class centralizes mapping decisions that balance safety, query performance, and cross-provider compatibility. The System column is constrained to 128 characters to prevent very large strings from inflating index sizes; Metric is left without a max length so callers can persist JSON payloads (including occasional large failure messages or request bodies). Indexes are chosen to optimize the two primary access patterns: recent entries for a specific system (or system prefix) and global time-based cleanup.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new MetricEntryConfiguration());
}
```

## Notes
- System has a max length of 128; changing this requires a schema migration and may affect index size and performance.
- Metric is stored as JSON (TEXT on SQLite). SQLite does not have a native JSON column type but supports json_extract/json_each for SQL-side queries.
- The compound index is defined as (System, CreatedAt) ascending; SQLite can satisfy DESC order via backward index scans, but other providers may have different behaviors—verify query plans if you rely on index-backed ORDER BY CreatedAt DESC.
- All configured properties (System, Metric, CreatedAt) are required (non-nullable) at the EF model level; ensure callers supply these values to avoid runtime errors.
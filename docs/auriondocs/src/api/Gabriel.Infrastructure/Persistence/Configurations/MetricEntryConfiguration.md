# MetricEntryConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/MetricEntryConfiguration.cs`  
> **Kind:** class

Configures how the MetricEntry entity maps to the database: table name, primary key, property constraints, and indexes tuned for the common read and cleanup patterns (recent entries per system and time-based deletions). Reach for this class when registering entity mappings in your DbContext so the MetricEntry table has predictable column constraints and indexes.

## Remarks
Centralizes storage and performance choices for metric events: limits the System column to 128 characters to avoid exploding index sizes, leaves Metric unconstrained so full JSON payloads (including error messages or request bodies) can be persisted, and adds a composite index on (System, CreatedAt) plus a standalone CreatedAt index. The composite index is intended to support queries that fetch recent entries for a specific system (and prefix-style system queries); the separate CreatedAt index supports efficient bulk deletes of old rows.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new MetricEntryConfiguration());
}
```

## Notes
- Metric is not length-constrained by this configuration; large JSON payloads are allowed and can grow row size and storage cost.
- System is limited to 128 characters to bound index key size; changing this requires a database migration and may affect index storage.
- The composite index is added without explicit ordering; the code relies on common SQLite behavior (backward index scan) to serve DESC-order queries — verify behavior on other database providers if you depend on index ordering.
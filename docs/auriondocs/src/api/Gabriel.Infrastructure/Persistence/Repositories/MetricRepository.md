# MetricRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs`  
> **Kind:** class

An EF Core-backed implementation of IMetricRepository that persists, queries and cleans up metric events. Use this repository when metrics should be recorded immediately and independently of business transactions (i.e., metrics are treated as best-effort, stand-alone events rather than part of a unit-of-work).

## Remarks
This class optimizes for three common metric scenarios: append (AddAsync), recent reads (RecentAsync / RecentByPrefixAsync) and bulk cleanup (DeleteOlderThanAsync). Writes call SaveChangesAsync inline so metric inserts are committed immediately rather than being deferred to an external unit-of-work. Read methods use AsNoTracking() to avoid change-tracking overhead for simple, read-only queries. DeleteOlderThanAsync uses ExecuteDeleteAsync to perform a single server-side bulk delete without materializing entities.

## Example
```csharp
// Add a metric and read the most recent 10 for a system
var repo = new MetricRepository(dbContext);
await repo.AddAsync(new MetricEntry { System = "payments", CreatedAt = DateTimeOffset.UtcNow, /* ... */ });
var recent = await repo.RecentAsync("payments", 10);

// Cleanup old metrics
var removed = await repo.DeleteOlderThanAsync(DateTimeOffset.UtcNow.AddMonths(-1));
```

## Notes
- AddAsync calls SaveChangesAsync immediately; it does not participate in a caller-managed unit-of-work. If you need metric inserts to be atomic with other work, do not use this repository as-is.
- Recent* methods return entities with AsNoTracking() — returned MetricEntry instances are not tracked by EF Core and changes to them will not be persisted unless explicitly attached and saved.
- RecentByPrefixAsync relies on StartsWith which EF Core translates to a SQL LIKE; on providers such as SQLite this can use an index when the prefix is a literal (no leading wildcard).
- DeleteOlderThanAsync uses ExecuteDeleteAsync which issues a direct bulk DELETE; it bypasses EF Core change-tracking and does not invoke entity lifecycle callbacks, but returns the number of rows deleted.
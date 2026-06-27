# MetricRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs`  
> **Kind:** class

EF Core-backed implementation of IMetricRepository that persists MetricEntry records into the application's AppDbContext. Use this when you need to record lightweight, standalone metric events that should be written immediately and independently of any business transaction.

## Remarks
This repository treats metrics as fire-and-forget events: writes call AddAsync followed by SaveChangesAsync directly (no Unit of Work) because metric recording should not block or participate in the surrounding business transaction. Read methods use AsNoTracking for cheap, read-only queries, and cleanup uses EF Core's bulk ExecuteDeleteAsync to avoid materializing entities during deletions.

## Example
```csharp
// assume `db` is an AppDbContext resolved from DI
var repo = new MetricRepository(db);

// add a metric
await repo.AddAsync(new MetricEntry { System = "orders", CreatedAt = DateTimeOffset.UtcNow, /* other props */ });

// read recent metrics for a system
var recent = await repo.RecentAsync("orders", limit: 50);

// read recent metrics for systems with a prefix
var prefixed = await repo.RecentByPrefixAsync("order", limit: 20);

// delete old metrics
int removed = await repo.DeleteOlderThanAsync(DateTimeOffset.UtcNow.AddMonths(-3));
```

## Notes
- If limit <= 0 the Recent* methods return an empty array immediately (no DB call). 
- AddAsync writes are committed inline via SaveChangesAsync; failures will throw and are not wrapped in a higher-level transaction — this is intentional so metric failures can be tolerated separately from business operations.
- RecentByPrefixAsync uses StartsWith which EF Core translates to a SQL LIKE; the database can use an index only when the pattern is a prefix (no leading wildcard).
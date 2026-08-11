# MetricRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs`  
> **Kind:** class

```csharp
public sealed class MetricRepository : IMetricRepository
```


MetricRepository is the EF Core-based persistence implementation of IMetricRepository. It writes MetricEntry records directly via AddAsync followed by SaveChangesAsync, treating metrics as standalone events that should not participate in a surrounding unit of work. This keeps metric emission decoupled from business transactions and prevents metric failures from cascading to the main operation.

## Remarks

This class acts as the persistence adapter between the domain concept of a metric and its database representation. Reads are optimized for concurrency and performance by using AsNoTracking, and queries rely on standard EF Core operators (equality filters, ordering by CreatedAt, and limitations via Take). The prefix-based query uses StartsWith, which EF translates to SQL LIKE; on SQLite, a literal prefix enables index usage for improved performance. For cleanup, DeleteOlderThanAsync uses a bulk delete via ExecuteDeleteAsync, avoiding entity materialization and maximizing efficiency for maintenance tasks.

## Example

```csharp
// Example usage of MetricRepository
var repo = new MetricRepository(db);

var entry = new MetricEntry().Create("PaymentsService", "{\"latencyMs\":123}");
await repo.AddAsync(entry, ct);

var recent = await repo.RecentAsync("PaymentsService", 10, ct);

var prefixRecent = await repo.RecentByPrefixAsync("Payments", 5, ct);

var deletedCount = await repo.DeleteOlderThanAsync(DateTimeOffset.UtcNow.AddDays(-7), ct);
```

## Notes

- Metrics are emitted as standalone events; a failure to persist should not roll back the primary business operation. Handle exceptions accordingly if you rely on metrics in critical workflows.
- Be aware that RecentByPrefixAsync relies on EF Core's translation of StartsWith to SQL LIKE. On SQLite, using a literal prefix can leverage an index for better performance.
- DeleteOlderThanAsync performs a bulk deletion without loading entities into memory, which is efficient for log cleanup but should be used with care if you rely on cascading side effects.

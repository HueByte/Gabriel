# MetricRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs`  
> **Kind:** class

```csharp
public sealed class MetricRepository : IMetricRepository
```


MetricRepository is the EF Core implementation of IMetricRepository. It persists metric events directly through the AppDbContext by invoking AddAsync followed by SaveChangesAsync, intentionally bypassing a unit-of-work wrapper since metric events are standalone and should not impact surrounding business transactions. The repository exposes methods to add a metric entry, fetch recent entries for a system or for a system name prefix, and perform a bulk cleanup of old metrics, all using asynchronous EF Core operations.

## Remarks
This abstraction isolates EF Core persistence concerns from domain logic, enabling straightforward recording and querying of metric events without participating in larger business transactions. Read paths favor no-tracking queries for performance, and deletions use bulk execution to avoid materializing entities. The design assumes metrics are eventually consistent enough for monitoring purposes and that occasional metric loss (when a commit fails) is acceptable because it does not affect the primary business operation. The prefix-based query relies on EF Core translation (StartsWith) to implement efficient, index-friendly filtering on supported databases.

## Notes
- Both RecentAsync and RecentByPrefixAsync guard against non-positive limits by returning an empty array, avoiding unnecessary queries. 
- DeleteOlderThanAsync performs a bulk DELETE via ExecuteDeleteAsync, avoiding entity tracking and in-memory materialization for efficient cleanup.
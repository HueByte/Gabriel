# MetricRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/MetricRepository.cs`  
> **Kind:** class

```csharp
public sealed class MetricRepository : IMetricRepository
```


MetricRepository is the EF Core implementation of IMetricRepository. It writes metrics inline by calling AddAsync on the MetricEntry DbSet and then SaveChangesAsync, because metric events are standalone and should not participate in a surrounding business transaction. The repository exposes AddAsync for persisting a new metric, RecentAsync for retrieving the most recent metrics for a given system, RecentByPrefixAsync for systems whose System value starts with a given prefix, and DeleteOlderThanAsync for bulk cleanup of old metric entries.

## Remarks
By isolating EF Core specifics behind IMetricRepository, the rest of the code interacts with metrics without needing persistence details. Reads are performed with AsNoTracking to avoid unnecessary change tracking, and RecentByPrefixAsync relies on EF Core translating StartsWith into a SQL LIKE predicate to enable index-friendly queries on supported databases. DeleteOlderThanAsync uses a single bulk DELETE via ExecuteDeleteAsync, which is efficient for cleanup tasks but does not materialize entities.

## Notes
- RecentAsync and RecentByPrefixAsync return an empty array when limit <= 0.
- StartsWith translates to LIKE in SQL; for SQLite, supply a literal prefix to leverage indices.
- DeleteOlderThanAsync executes a bulk operation with no entity tracking; no in-memory materialization is performed.
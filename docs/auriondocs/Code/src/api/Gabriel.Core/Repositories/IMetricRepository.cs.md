# IMetricRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`  
> **Kind:** interface

```csharp
public interface IMetricRepository
```


IMetricRepository defines how metric event entries are stored and retrieved. This repository serves as the storage contract for the metric log: reads power the diagnostic surface (controllers and support tooling), while writes are routed through the IMetricRecorder service in Engine so subsystems don’t depend on EF directly. Callers provide a MetricEntry that has already been serialized to JSON; AddAsync persists a single row in a schema-less store. The retrieval methods enable diagnostics workflows: RecentAsync returns the most recent entries for a specific system, while RecentByPrefixAsync aggregates entries across systems whose names start with a given prefix; DeleteOlderThanAsync supports cleanup by removing data older than the cutoff, intended for maintenance tasks rather than HTTP-exposed operations.

## Remarks
As an abstraction, this interface enables swapping the underlying storage backend and facilitates testing with in-memory or alternative implementations without touching business logic. The schema-less design offers flexibility for evolving metric payloads while preserving a stable contract for readers. The read endpoints are tailored for diagnostics tooling, returning ordered results to support intuitive analysis across systems and prefixes.

## Notes
- DeleteOlderThanAsync performs a hard delete and should be used with caution, typically from maintenance/background tasks in line with retention policies.
- Pass the CancellationToken through to the underlying I/O operations to support cancellation and cancellation-aware shutdown.
- For RecentByPrefixAsync, the limit applies to the combined result set across all matched systems, not per-system; callers should account for cross-system aggregation when presenting results.
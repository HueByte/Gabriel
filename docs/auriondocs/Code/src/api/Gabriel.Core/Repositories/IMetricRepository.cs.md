# IMetricRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`  
> **Kind:** interface

```csharp
public interface IMetricRepository
```


IMetricRepository is a storage contract for metric event logs. It defines how a diagnostic surface reads stored metrics and how the writer-side records them via the IMetricRecorder in Engine, abstracting away the underlying persistence (EF) details. Implementations expose asynchronous operations to persist a MetricEntry payload (already serialized to JSON) and to retrieve recent entries, enabling efficient diagnostics views without leaking storage specifics. The interface is schema-less on the storage side, with MetricEntry carrying the essential metadata (Id, System, Metric, CreatedAt).

## Remarks
This abstraction separates concerns between the diagnostic tooling and the persistence layer, allowing subsystems to emit metrics without depending directly on EF or a particular database schema. It provides targeted read paths for diagnostics endpoints: RecentAsync focuses on a single system, while RecentByPrefixAsync enables querying across many systems by a common prefix, reducing the risk of N+1 round-trips on the read side. DeleteOlderThanAsync enables maintenance cleanups without exposing HTTP endpoints; it is intended for background tasks or manual maintenance scripts.

## Notes
- DeleteOlderThanAsync returns the number of rows deleted; use this to gauge maintenance impact.
- All methods accept a CancellationToken; pass a token from callsites to participate in cooperative cancellation.
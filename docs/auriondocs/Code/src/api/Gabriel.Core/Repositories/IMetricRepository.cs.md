# IMetricRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`  
> **Kind:** interface

```csharp
public interface IMetricRepository
```


IMetricRepository defines the storage contract for the generic metric event log. It exposes persistence and read operations used by diagnostics surfaces, while the writer side is wired through IMetricRecorder in Engine so subsystems don't talk to EF directly. Implementations persist metric entries serialized to JSON into a schema-less store and offer targeted read paths for diagnostics: per-system recent events via RecentAsync, and cross-system queries via RecentByPrefixAsync. A maintenance operation is provided to purge old data via DeleteOlderThanAsync.

Use AddAsync to persist a single metric entry after serializing the payload to JSON; use RecentAsync to retrieve the most recent entries for a specific system, and use RecentByPrefixAsync to fetch recent events across systems that share a common prefix; use DeleteOlderThanAsync in background maintenance to remove stale data. This interface abstracts storage concerns away from consumers, allowing the diagnostic endpoints and tooling to evolve without tying them to a particular ORM or data-store.

## Remarks
IMetricRepository abstracts persistence from the rest of the telemetry pipeline, enabling the read paths used by diagnostics to be backed by any suitable storage. It decouples the web/diagnostics endpoints from the writer logic (MetricRecorder) and from EF concerns, promoting testability and flexibility in choosing or evolving the data store.

## Notes
- The limit semantics in RecentByPrefixAsync apply to the total result set across all matched systems, not per-system; UI layers should group entries by system after retrieval.
- DeleteOlderThanAsync is a hard delete intended for maintenance tasks and is not exposed via HTTP; use with care to avoid unintended data loss.
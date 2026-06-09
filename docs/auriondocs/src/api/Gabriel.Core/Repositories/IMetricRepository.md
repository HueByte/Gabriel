# IMetricRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`  
> **Kind:** interface

Persistence contract for the generic metric event log used by diagnostic surfaces and maintenance tasks. Implementations store MetricEntry rows (the caller serializes payloads to JSON) and expose async operations for appending entries, reading the most-recent entries for a specific system or a system-name prefix, and removing old rows — use this interface when you need durable storage or read access for diagnostics rather than recording metrics via IMetricRecorder.

## Remarks
This interface separates the storage concerns from the metric-producing code: writers typically use a higher-level IMetricRecorder, while controllers and tooling read via IMetricRepository. The storage layer is intentionally schema-less — entries contain JSON payloads — and the read APIs are tuned for diagnostics: RecentAsync returns newest-first rows for one exact system, RecentByPrefixAsync returns newest-first rows across all systems matching a prefix (with a combined limit to avoid N+1 queries), and DeleteOlderThanAsync supports background cleanup/maintenance.

## Example
```csharp
// Append a metric
await metricRepository.AddAsync(new MetricEntry { System = "web_search.indexer", PayloadJson = json }, ct);

// Read the 50 most recent entries for one subsystem
var recentForSystem = await metricRepository.RecentAsync("web_search.indexer", 50, ct);

// Read the 100 most recent entries across any system starting with "web_search."
var recentAcrossPrefix = await metricRepository.RecentByPrefixAsync("web_search.", 100, ct);

// Background cleanup: delete all rows older than 30 days
var cutoff = DateTimeOffset.UtcNow.AddDays(-30);
int deleted = await metricRepository.DeleteOlderThanAsync(cutoff, ct);
```

## Notes
- RecentAsync and RecentByPrefixAsync return newest-first ordering.
- RecentByPrefixAsync's limit applies to the combined result set (not per-system); this is deliberate to avoid N+1 queries when aggregating multiple subsystems.
- Callers must serialize payloads to JSON before calling AddAsync — the storage layer expects schema-less JSON payloads.
- All methods are asynchronous and accept a CancellationToken; DeleteOlderThanAsync returns the number of rows deleted and is intended for background or manual maintenance (not exposed over HTTP).
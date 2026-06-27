# IMetricRepository

> **File:** `src/api/Gabriel.Core/Repositories/IMetricRepository.cs`  
> **Kind:** interface

Storage contract for a generic metric/event log used by diagnostic readers and background maintenance tasks. Use this interface when you need a persistent, schema-less store for metric entries that will be written by recording services (e.g. IMetricRecorder) and read by diagnostics controllers or tooling.

## Remarks
This interface separates the writer-side recording API from the storage implementation used by diagnostics and maintenance code. Implementations can use EF, a document store, or any other backing store because the repository treats payloads as opaque JSON (schema-less). The RecentByPrefixAsync method is provided to avoid N+1 reads when consumers need "everything under a subsystem prefix" in a single query.

## Example
```csharp
// Persisting a metric (caller serializes payload into MetricEntry.Payload as JSON)
var entry = new MetricEntry { System = "web_search.indexer", Timestamp = DateTimeOffset.UtcNow, Payload = jsonPayload };
await metricRepository.AddAsync(entry, cancellationToken);

// Reading recent metrics for a subsystem prefix and grouping by exact system name
var recent = await metricRepository.RecentByPrefixAsync("web_search.", 200, cancellationToken);
var grouped = recent.GroupBy(e => e.System)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(e => e.Timestamp).ToList());
```

## Notes
- AddAsync expects the MetricEntry to already contain a JSON-serialized payload; the storage layer does not enforce or interpret a schema.  
- RecentAsync and RecentByPrefixAsync return results newest-first; callers should not rely on any other implicit ordering.  
- The limit parameter on RecentByPrefixAsync applies to the combined result set across all matching systems (not per-system).  
- DeleteOlderThanAsync performs a hard delete and is intended for background maintenance or manual cleanup (not exposed via HTTP).
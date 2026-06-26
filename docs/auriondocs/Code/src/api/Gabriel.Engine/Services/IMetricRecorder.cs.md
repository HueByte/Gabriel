# IMetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/IMetricRecorder.cs`  
> **Kind:** interface

A lightweight abstraction for recording telemetry-style metric events from engine subsystems. Use this interface when you want to emit a named metric (a stable dotted "system" identifier) with an arbitrary payload without taking a dependency on the underlying storage implementation or risking metric-related exceptions propagating into business logic.

## Remarks
This interface decouples producers of metric data from the storage and serialization concerns. Implementations are expected to serialize the provided payload to JSON and persist it to the metric event log; they also absorb storage/IO errors so that metric recording failures do not interrupt the caller's flow. The contract is intentionally minimal: callers supply a stable "system" name and a payload whose shape is defined by the producing subsystem.

## Example
```csharp
// Typical usage from an async subsystem method
public async Task SearchAndRecordAsync(string query, CancellationToken ct)
{
    var metric = new {
        outcome = "success",
        query = query,
        result_count = 5,
        latency_ms = 287
    };

    // _metrics is an IMetricRecorder injected into the subsystem
    await _metrics.RecordAsync("web_search.tavily", metric, ct);
}
```

## Notes
- The payload is serialized to JSON by implementations, so ensure the object is JSON-serializable (avoid circular references, unsupported types).
- The "system" parameter should be a stable, dotted identifier (for example: "web_search.tavily") so downstream tooling can group and aggregate events reliably.
- Implementations absorb storage errors; callers should not assume recording succeeded — metrics are best-effort and must not be relied upon for control flow or correctness.
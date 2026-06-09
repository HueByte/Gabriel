# IMetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/IMetricRecorder.cs`  
> **Kind:** interface

Record asynchronous metric events under a stable, dotted "system" name. Use this interface when subsystems need a low-cost, best-effort way to emit structured metrics (for aggregation and analysis) without allowing metric write failures to affect the caller's primary control flow.

## Remarks
This abstraction decouples producers of metric payloads from the underlying storage repository and read-side tooling. Implementations are expected to serialize the generic payload (commonly to JSON), group events by the provided system identifier, and avoid raising exceptions that would interfere with business logic — i.e., metric writes are best-effort. Conventions for the payload include adding an outcome field (e.g. "success", "error", "empty") so downstream aggregators can count events without parsing every field.

## Example
```csharp
await _metrics.RecordAsync("web_search.tavily", new {
    outcome = "success",
    query = "best pizza near me",
    result_count = 5,
    latency_ms = 287
}, ct);
```

## Notes
- Implementations may absorb storage errors; callers should not rely on RecordAsync to guarantee persistence of metrics.
- The generic payload T must be serializable by the concrete recorder (typically JSON); avoid non-serializable or very large objects.
- Respect the CancellationToken: if the token is canceled the operation may complete as canceled and the metric might not be written.
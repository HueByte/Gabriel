# IMetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/IMetricRecorder.cs`  
> **Kind:** interface

```csharp
public interface IMetricRecorder
```


IMetricRecorder provides a lightweight wrapper around the engine's metric log. Subsystems should use RecordAsync instead of talking directly to storage; it serializes the payload to JSON and swallows storage errors so metric writes never affect business logic, and it relies on a stable dotted system name (for example, "web_search.tavily") to enable grouping by read-side tooling. The payload shape is flexible and specific to the subsystem; by convention include an `outcome` field with values ("success", "error", or "empty") so aggregations can count without inspecting every field.

## Remarks
By centralizing metric emission behind this interface, the implementation decouples callers from storage details and ensures a consistent, best-effort telemetry path across subsystems. The generic T payload supports strong typing while preserving the ability to evolve payload shapes independently of the interface.

## Example
```csharp
await _metrics.RecordAsync("web_search.tavily", new {
    outcome = "success",
    query = "example query",
    result_count = 5,
    latency_ms = 287
}, ct);
```

## Notes
- Metric writes are best-effort: failures are swallowed and do not affect business logic. Do not rely on metrics for correctness or user-facing behavior.
- Payload types must be JSON-serializable. Avoid non-serializable references or large object graphs; keep payloads focused and simple.

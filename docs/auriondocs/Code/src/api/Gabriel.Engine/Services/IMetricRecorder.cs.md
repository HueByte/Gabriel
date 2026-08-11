# IMetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/IMetricRecorder.cs`  
> **Kind:** interface

```csharp
public interface IMetricRecorder
```


IMetricRecorder provides a convenient surface for writing metric events to the generic metric event log. Subsystems should use this interface rather than depending directly on the storage repository; it serializes the payload to JSON and absorbs storage errors so a metric-record failure never bubbles up into the caller's business path.

## Remarks
This abstraction decouples metric reporting from the storage implementation and makes metric writes best-effort. The stable, dotted system name (e.g. web_search.tavily, agent_loop.iteration) lets read-side tooling group and analyze events consistently, while the flexible payload lets subsystems evolve what they measure without API changes. The inclusion of an outcome field (success | error | empty) is a common convention to support high-level aggregation without inspecting every payload attribute.

## Example
```csharp
// Wire example
var payload = new Dictionary<string, object>
{
  ["outcome"] = "success",
  ["query"] = "...",
  ["result_count"] = 5,
  ["latency_ms"] = 287
};
await recorder.RecordAsync("web_search.tavily", payload, CancellationToken.None);
```

## Notes
- Writes are best-effort; metric storage failures are swallowed by design and should not affect business logic.
- The payload must be JSON-serializable; using a `Dictionary<string, object>` ensures compatibility with common JSON serializers.
- The CancellationToken parameter is optional; pass a token if you need to cancel the write.
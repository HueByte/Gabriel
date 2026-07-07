# IMetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/IMetricRecorder.cs`  
> **Kind:** interface

```csharp
public interface IMetricRecorder
```


IMetricRecorder is a convenient abstraction that lets subsystems emit metrics to the central event log without depending on the storage layer. It serializes the payload to JSON and absorbs storage errors so a metric write never bubbles up into business logic.

## Remarks
By decoupling metric emission from storage, this interface provides a stable write surface that front-ends, services, and workers can rely on. The system name (a stable dotted identifier) enables consistent grouping and querying by read-side tooling, while allowing the payload to evolve per subsystem; conventionally include an outcome field to support simple aggregations.

## Example
```csharp
// Wire example (per web-search call)
await _metrics.RecordAsync("web_search.tavily", new {
    outcome = "success",
    query = "...",
    result_count = 5,
    latency_ms = 287
}, ct);
```

## Notes
- Do not rely on metric writes for business logic failure handling; errors are swallowed.
- Payload type T must be serializable to JSON; ensure no non-serializable fields (e.g., circular references).
- The system identifier should be stable; changing it is a breaking change for analytics.
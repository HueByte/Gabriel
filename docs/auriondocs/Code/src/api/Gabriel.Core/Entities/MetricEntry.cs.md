# MetricEntry

> **File:** `src/api/Gabriel.Core/Entities/MetricEntry.cs`  
> **Kind:** class

```csharp
public class MetricEntry
```


MetricEntry is a single, persisted time-series telemetry row that ties a subsystem (System) to a JSON-encoded Metric payload. It auto-assigns a unique Id and stamps CreatedAt at creation; use MetricEntry.Create to validate inputs and initialize a new, immutable entry for persistence.

## Remarks
This lightweight abstraction encapsulates the invariants around telemetry rows: a non-empty System and a non-empty Metric JSON payload are required, and CreatedAt cannot be backfilled, ensuring reliable chronology. By making the constructor private and funneling creation through Create, the class guarantees consistent initialization of Id and CreatedAt on every instance, making it safe to persist as a stable data transfer object.

## Example
```csharp
// Typical usage
var entry = MetricEntry.Create("web_search.tavily", "{\"latencyMs\":123}");
Console.WriteLine(entry.Id);
Console.WriteLine(entry.CreatedAt);
```

## Notes
- The Metric payload is stored as a JSON string; this class does not parse or validate JSON.
- System is trimmed to remove leading/trailing whitespace and cannot be null or whitespace; Create enforces this invariant.
- CreatedAt is set to the moment of construction (UTC) and is the primary time-based ordering key; Id is just a GUID for consistency.
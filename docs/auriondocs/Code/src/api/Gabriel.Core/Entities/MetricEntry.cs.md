# MetricEntry

> **File:** `src/api/Gabriel.Core/Entities/MetricEntry.cs`  
> **Kind:** class

```csharp
public class MetricEntry
```


MetricEntry is a generic time-series event row used by all subsystems that persist telemetry. It stores two semantic columns: System, a stable dotted name identifying the subsystem, and Metric, a JSON payload whose shape is defined by the writer; the storage layer is schema-less so new telemetry shapes can be recorded without migrations. CreatedAt is auto-stamped on construction and Id is a Guid, ensuring predictable ordering by CreatedAt and a consistent identity.

The class is intentionally constructed via the Create factory, which validates inputs and initializes required fields. Call MetricEntry.Create(system, metricJson) to record a new event; direct construction is private to guarantee that both System and Metric are provided and that CreatedAt/Id are always populated.

## Remarks
MetricEntry represents a lightweight, schema-flexible telemetry row: you can query by System (exact name or prefix) and store arbitrary JSON in Metric for downstream analysis. The combination of System and Metric enables robust, schema-less growth of telemetry without migrations, while CreatedAt provides a reliable temporal ordering and Id ensures stable identity across the system.

## Example
```csharp
var entry = MetricEntry.Create("web_search.tavily", "{\"latencyMs\": 120, \"count\": 3}");
```

## Notes
- The Create method enforces non-empty System and Metric JSON; passing null or whitespace throws ArgumentException. 
- The Metric property stores the provided JSON string verbatim; there is no JSON validation performed by MetricEntry itself.
- The constructor is private, so all instances must be created via Create, ensuring consistent initialization of CreatedAt and Id.
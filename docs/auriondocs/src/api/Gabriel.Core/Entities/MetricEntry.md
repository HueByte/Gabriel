# MetricEntry

> **File:** `src/api/Gabriel.Core/Entities/MetricEntry.cs`  
> **Kind:** class

Represents a single persisted telemetry/time-series event used by subsystems to record metrics. Use MetricEntry.Create(system, metricJson) when producing rows for the shared storage layer; the instance is stamped with CreatedAt at construction and is intended to be immutable from callers.

## Remarks
This class provides a schema-less container for telemetry: System is a stable dotted name that lets readers filter or prefix-query events, while Metric is an opaque JSON document whose schema is owned by the producing subsystem. CreatedAt is auto-populated to enforce natural ordering and prevent callers from backfilling or reordering rows; Id is a Guid included for consistency with the database schema but ordering should be based on CreatedAt. The private constructor and the static Create factory centralize validation (System and Metric are required) so callers cannot create invalid entries.

## Example
```csharp
// Create a metric row for the "web_search.tavily" subsystem
var metricJson = "{ \"queryCount\": 42, \"latencyMs\": 123 }";
var row = MetricEntry.Create("web_search.tavily", metricJson);

// row.Id => Guid, row.CreatedAt => timestamp (UTC), row.System => "web_search.tavily", row.Metric => metricJson
// Persist `row` using your storage layer (not shown).
```

## Notes
- Metric is stored as a raw JSON string; MetricEntry does not parse or validate the JSON content.
- System is trimmed and validated by Create; passing null/empty/whitespace throws ArgumentException.
- CreatedAt is set when the instance is constructed and cannot be changed through the public API — do not rely on callers to set this value.
- Some serializers/deserializers may bypass the factory validation and set properties directly; if you deserialize from untrusted data, validate the resulting instance before trusting its fields.

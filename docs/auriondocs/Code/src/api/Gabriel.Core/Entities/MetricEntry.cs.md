# MetricEntry

> **File:** `src/api/Gabriel.Core/Entities/MetricEntry.cs`  
> **Kind:** class

```csharp
public class MetricEntry
```


Represents a single persisted telemetry entry used by all time-series subsystems. It stores a stable System identifier and a free-form Metric JSON payload, enabling new subsystems to emit data without migrations. CreatedAt is auto-stamped on creation and Id is a Guid; construction is private to enforce invariants via the Create factory.

## Remarks
MetricEntry isolates the event source (System) from its payload (Metric), supporting a schema-less telemetry store while preserving a traceable identity. The Create factory enforces basic invariants (non-empty system and metric) and trims whitespace to ensure consistent data at write time. Public getters with private setters make instances effectively immutable after creation, simplifying correctness in concurrent or persisted contexts.

## Example
```csharp
var entry = MetricEntry.Create("web_search.tavily", "{\"latencyMs\":42}");
```

## Notes
- The Metric field holds arbitrary JSON as a string; downstream code must parse it as needed, since there is no enforced schema here.
- The Create method validates inputs and trims the System name; invalid calls throw ArgumentException.
- CreatedAt is fixed at creation time and Id is generated once; these fields are immutable after construction.
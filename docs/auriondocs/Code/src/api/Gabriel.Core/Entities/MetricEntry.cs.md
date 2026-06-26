# MetricEntry

> **File:** `src/api/Gabriel.Core/Entities/MetricEntry.cs`  
> **Kind:** class

```csharp
// Generic time-series event row used by every subsystem that wants persisted
// telemetry. Two semantic columns:
//   - System: a stable dotted name correlating events to a subsystem (e.g.
//     "web_search.tavily", "agent_loop.iteration", "memory.save"). Querying
//     by exact name or by prefix is the read-side workflow.
//   - Metric: a JSON document carrying whatever shape that subsystem cares
//     about. Schema lives in the code that writes it; the storage layer is
//     deliberately schema-less so a new subsystem can start recording without
//     a migration.
//
// CreatedAt is auto-stamped on construction so callers can't backfill or
// reorder rows. Id is a Guid for consistency with the rest of the schema
// (the natural ordering is by CreatedAt, not by Id).
public class MetricEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string System { get; private set; } = string.Empty;
    public string Metric { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private MetricEntry() { }

    public static MetricEntry Create(string system, string metricJson)
    {
        if (string.IsNullOrWhiteSpace(system))
            throw new ArgumentException("System is required.", nameof(system));
        if (string.IsNullOrWhiteSpace(metricJson))
            throw new ArgumentException("Metric JSON is required.", nameof(metricJson));

        return new MetricEntry
        {
            System = system.Trim(),
            Metric = metricJson,
        };
    }
}
```


A lightweight, schema-less time-series row used to persist telemetry from multiple subsystems. Use this when you need to record an event or metric as an opaque JSON payload tied to a stable subsystem name; the entry is stamped with a UTC creation time and a GUID identifier on construction.

## Remarks
This class provides a uniform storage shape for telemetry: a stable dotted "System" name for querying (exact or prefix searches) plus a free-form JSON "Metric" blob whose shape is defined by the emitting subsystem. CreatedAt is intentionally auto-stamped to enforce natural ordering and to prevent callers from backfilling or reordering rows. The actual JSON schema and interpretation live in the producers/consumers of the Metric string — the persistence layer deliberately does not validate or impose a schema.

## Example
```csharp
// Create a telemetry row for the web_search subsystem
var metricJson = "{\"latencyMs\": 123, \"success\": true}";
var entry = MetricEntry.Create("web_search.tavily", metricJson);

// Persist entry with your repository/ORM
repository.Add(entry);
await repository.SaveChangesAsync();
```

## Notes
- CreatedAt and Id are set internally (UTC and Guid.NewGuid()); callers cannot supply or override them, so tests or migrations that need deterministic timestamps must accommodate that.
- Metric is stored as an opaque string: the class does not validate JSON or enforce a schema — callers are responsible for producing well-formed JSON and for versioning any schema changes.
- System is required, trimmed, and cannot be empty or whitespace; use a stable dotted name (e.g. "agent_loop.iteration") to make queries and prefixes meaningful.
namespace Gabriel.Core.Entities;

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

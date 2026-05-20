using Gabriel.Core.Entities;

namespace Gabriel.Core.Repositories;

// Storage contract for the generic metric event log. Reads are the diagnostic
// surface (controllers, support tooling); the writer-side is fronted by the
// IMetricRecorder service in Engine so subsystems don't have to talk to EF
// directly.
public interface IMetricRepository
{
    // Persists a single row. Caller has already serialized the payload to
    // JSON; the storage layer is schema-less.
    Task AddAsync(MetricEntry entry, CancellationToken ct = default);

    // Most recent N rows for an exact system name, newest first. Used by
    // diagnostics endpoints that target one subsystem.
    Task<IReadOnlyList<MetricEntry>> RecentAsync(string system, int limit, CancellationToken ct = default);

    // Most recent N rows across every system whose name starts with the given
    // prefix, newest first. Lets the web-search diagnostics endpoint pull
    // "everything under web_search." in one query and then group by system on
    // the read side - avoids N+1 round-trips when there are several providers.
    // The limit applies to the combined result set, not per-system.
    Task<IReadOnlyList<MetricEntry>> RecentByPrefixAsync(string systemPrefix, int limit, CancellationToken ct = default);

    // Hard delete for cleanup tasks. Not exposed via HTTP - call from a
    // background service or a manual maintenance script.
    Task<int> DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct = default);
}

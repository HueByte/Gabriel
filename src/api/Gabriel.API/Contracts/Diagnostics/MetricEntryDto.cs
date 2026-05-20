using System.Text.Json;

namespace Gabriel.API.Contracts.Diagnostics;

// Wire shape of one row in the generic metric event log. The Metric column
// is raw JSON; we ship it as an opaque JsonElement so the caller can
// re-deserialize against whatever shape the originating subsystem agreed on.
public sealed record MetricEntryDto(
    Guid Id,
    string System,
    JsonElement Metric,
    DateTimeOffset CreatedAt);

// Wire shape of GET /diagnostics/metrics. Returns up to `limit` recent rows
// matching the requested system (exact match) or system prefix, newest first.
public sealed record MetricEntriesResponse(
    IReadOnlyList<MetricEntryDto> Entries,
    int Count);

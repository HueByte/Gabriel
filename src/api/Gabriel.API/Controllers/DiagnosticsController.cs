using System.Text.Json;
using Gabriel.API.Contracts.Diagnostics;
using Gabriel.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

// Operational read-only diagnostics over the generic metric event log.
// Authenticated but not admin-gated - every user has a stake in knowing
// whether their tooling actually works, and nothing surfaced here is
// sensitive (no API keys, no per-user queries beyond what the caller's own
// searches generated).
[ApiController]
[Authorize]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
{
    private const string WebSearchSystemPrefix = "web_search.";

    private readonly IMetricRepository _metrics;

    public DiagnosticsController(IMetricRepository metrics)
    {
        _metrics = metrics;
    }

    // Per-provider health snapshot for the web_search tool. Pulls the most
    // recent `windowSize` rows under the "web_search." system prefix from
    // the metric log and aggregates them on the read side. Default window
    // covers a hobby user's typical day of searches; pass a larger one for
    // a longer-tail view.
    [HttpGet("web-search")]
    public async Task<ActionResult<WebSearchDiagnosticsResponse>> WebSearch(
        [FromQuery] int windowSize = 200,
        CancellationToken ct = default)
    {
        windowSize = Math.Clamp(windowSize, 10, 5000);
        var rows = await _metrics.RecentByPrefixAsync(WebSearchSystemPrefix, windowSize, ct);

        // Group raw rows by system name, aggregate each group into a stats
        // record. Per-row JSON is parsed lazily - we only touch the fields
        // we need.
        var grouped = rows.GroupBy(r => r.System, StringComparer.Ordinal);
        var providers = new List<WebSearchProviderStatsDto>();
        foreach (var group in grouped)
        {
            long total = 0, success = 0, error = 0, empty = 0;
            double totalLatencyMs = 0;
            DateTimeOffset? lastSuccess = null;
            DateTimeOffset? lastFailure = null;
            string? lastFailureQuery = null;
            string? lastFailureMessage = null;
            string? mostRecentOutcome = null;

            // group enumerates in source order = newest first (we ordered by
            // CreatedAt DESC in the repo). Track that first-seen-wins for
            // "most recent" fields.
            foreach (var row in group)
            {
                total++;
                using var doc = JsonDocument.Parse(row.Metric);
                var root = doc.RootElement;
                var outcome = root.TryGetProperty("outcome", out var o) ? o.GetString() : null;
                if (mostRecentOutcome is null) mostRecentOutcome = outcome;

                switch (outcome)
                {
                    case "success":
                        success++;
                        if (lastSuccess is null) lastSuccess = row.CreatedAt;
                        if (root.TryGetProperty("latency_ms", out var ms) && ms.ValueKind == JsonValueKind.Number)
                            totalLatencyMs += ms.GetDouble();
                        break;
                    case "empty":
                        // Empty is still a successful network call - latency
                        // counts; just no results.
                        success++;
                        empty++;
                        if (lastSuccess is null) lastSuccess = row.CreatedAt;
                        if (root.TryGetProperty("latency_ms", out var emptyMs) && emptyMs.ValueKind == JsonValueKind.Number)
                            totalLatencyMs += emptyMs.GetDouble();
                        break;
                    case "error":
                        error++;
                        if (lastFailure is null)
                        {
                            lastFailure = row.CreatedAt;
                            lastFailureQuery = root.TryGetProperty("query", out var q) ? q.GetString() : null;
                            lastFailureMessage = root.TryGetProperty("error_message", out var em) ? em.GetString() : null;
                        }
                        break;
                }
            }

            var providerName = group.Key.StartsWith(WebSearchSystemPrefix, StringComparison.Ordinal)
                ? group.Key[WebSearchSystemPrefix.Length..]
                : group.Key;
            var avg = success == 0 ? 0.0 : totalLatencyMs / success;
            var unhealthy = success == 0 || mostRecentOutcome == "error";

            providers.Add(new WebSearchProviderStatsDto(
                Provider: providerName,
                TotalCalls: total,
                SuccessfulCalls: success,
                ErrorCalls: error,
                EmptyCalls: empty,
                LastSuccessAt: lastSuccess,
                LastFailureAt: lastFailure,
                LastFailureQuery: lastFailureQuery,
                LastFailureMessage: lastFailureMessage,
                AvgLatencyMs: avg,
                IsUnhealthy: unhealthy));
        }

        providers.Sort((a, b) => StringComparer.Ordinal.Compare(a.Provider, b.Provider));

        return Ok(new WebSearchDiagnosticsResponse(
            Providers: providers,
            HasUnhealthyProvider: providers.Any(p => p.IsUnhealthy),
            WindowSize: windowSize));
    }

    // Generic browse over the metric event log. Lets the user see what's
    // being recorded under any subsystem - "GET /diagnostics/metrics?system=
    // web_search.tavily&limit=50" for the latest 50 Tavily events,
    // "&systemPrefix=agent_loop." to scan multiple related systems at once.
    // Exactly one of `system` / `systemPrefix` must be supplied.
    [HttpGet("metrics")]
    public async Task<ActionResult<MetricEntriesResponse>> Metrics(
        [FromQuery] string? system,
        [FromQuery] string? systemPrefix,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var hasExact = !string.IsNullOrWhiteSpace(system);
        var hasPrefix = !string.IsNullOrWhiteSpace(systemPrefix);
        if (hasExact == hasPrefix)
        {
            return BadRequest(new { error = "Provide exactly one of 'system' or 'systemPrefix'." });
        }

        limit = Math.Clamp(limit, 1, 1000);
        var rows = hasExact
            ? await _metrics.RecentAsync(system!.Trim(), limit, ct)
            : await _metrics.RecentByPrefixAsync(systemPrefix!.Trim(), limit, ct);

        var dtos = rows.Select(r => new MetricEntryDto(
            Id: r.Id,
            System: r.System,
            // Parse to JsonElement so the response is naturally JSON, not a
            // doubly-escaped string. JsonDocument's RootElement is owned by
            // the doc, which would be disposed before the response is
            // serialized - clone to detach.
            Metric: JsonDocument.Parse(r.Metric).RootElement.Clone(),
            CreatedAt: r.CreatedAt)).ToList();

        return Ok(new MetricEntriesResponse(Entries: dtos, Count: dtos.Count));
    }
}

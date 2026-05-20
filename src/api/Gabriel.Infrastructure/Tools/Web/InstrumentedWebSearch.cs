using System.Diagnostics;
using Gabriel.Engine.Services;
using Gabriel.Engine.Tools.Web;

namespace Gabriel.Infrastructure.Tools.Web;

// Decorator that records every IWebSearch call as a row in the generic metric
// event log. Wraps each provider at registration time so the composite path
// picks up per-provider events automatically, and single-provider mode is
// tracked too (you still want to know if your only configured backend stops
// working).
//
// One row per call. System name follows the convention "web_search.<provider>"
// (e.g. "web_search.tavily", "web_search.brave", "web_search.duckduckgo");
// payload carries outcome / query / result_count / latency_ms / error_message.
// The diagnostics endpoint pulls recent rows and aggregates them on read.
//
// The decorator does NOT swallow exceptions - it records the failure and
// rethrows. The composite catches at its own layer; single-provider callers
// see the original exception unchanged.
public sealed class InstrumentedWebSearch : IWebSearch
{
    private readonly IWebSearch _inner;
    private readonly IMetricRecorder _metrics;
    private readonly string _systemName;

    public InstrumentedWebSearch(IWebSearch inner, IMetricRecorder metrics, string providerName)
    {
        _inner = inner;
        _metrics = metrics;
        // Lowercased so "web_search.tavily" stays stable regardless of how the
        // DI registration cased the provider name. Read-side queries can rely
        // on a canonical shape.
        _systemName = $"web_search.{providerName.ToLowerInvariant()}";
    }

    public async Task<IReadOnlyList<WebSearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var results = await _inner.SearchAsync(query, limit, ct);
            sw.Stop();
            await _metrics.RecordAsync(_systemName, new WebSearchEvent(
                Outcome: results.Count == 0 ? "empty" : "success",
                Query: query,
                ResultCount: results.Count,
                LatencyMs: sw.Elapsed.TotalMilliseconds,
                ErrorMessage: null), ct);
            return results;
        }
        catch (OperationCanceledException)
        {
            // Cancellation isn't a provider failure - the caller pulled the
            // plug (user navigated away, request timed out at the agent
            // level). Don't pollute the log with it.
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            // Pass CancellationToken.None for the metric record: the original
            // ct may already be triggered (e.g. by the agent loop bailing on
            // this iteration) and we still want the failure persisted.
            await _metrics.RecordAsync(_systemName, new WebSearchEvent(
                Outcome: "error",
                Query: query,
                ResultCount: 0,
                LatencyMs: sw.Elapsed.TotalMilliseconds,
                ErrorMessage: ex.Message), CancellationToken.None);
            throw;
        }
    }

    // Payload shape recorded into MetricEntry.Metric. PropertyNamingPolicy on
    // the recorder turns these PascalCase names into snake_case JSON keys
    // (outcome, query, result_count, latency_ms, error_message).
    private sealed record WebSearchEvent(
        string Outcome,
        string Query,
        int ResultCount,
        double LatencyMs,
        string? ErrorMessage);
}

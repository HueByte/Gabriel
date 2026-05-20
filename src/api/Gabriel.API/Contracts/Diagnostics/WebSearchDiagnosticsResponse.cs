namespace Gabriel.API.Contracts.Diagnostics;

// Wire shape of GET /diagnostics/web-search. One entry per provider that has
// recorded events into the metric log within the requested lookback window.
public sealed record WebSearchDiagnosticsResponse(
    IReadOnlyList<WebSearchProviderStatsDto> Providers,
    // Convenience flag: true if any provider's most recent outcome was a
    // failure (or it has no successful calls in the window). UI uses it to
    // badge the search tool with a warning indicator.
    bool HasUnhealthyProvider,
    // The most-recent-N-events window the stats were computed over. Lets the
    // UI explain to the user "based on the last 200 search calls" rather
    // than leaving them to guess.
    int WindowSize);

public sealed record WebSearchProviderStatsDto(
    // Display name from the System column - "web_search.<provider>" stripped
    // of the prefix (e.g. "tavily", "brave", "duckduckgo").
    string Provider,
    long TotalCalls,
    long SuccessfulCalls,
    long ErrorCalls,
    long EmptyCalls,
    DateTimeOffset? LastSuccessAt,
    DateTimeOffset? LastFailureAt,
    string? LastFailureQuery,
    string? LastFailureMessage,
    double AvgLatencyMs,
    // True when the provider has recorded events in the window and either
    // has zero successful calls, or its most recent event was a failure.
    bool IsUnhealthy);

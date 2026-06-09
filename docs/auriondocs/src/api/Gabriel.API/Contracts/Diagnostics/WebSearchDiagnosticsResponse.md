# WebSearchDiagnosticsResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`

## Contents

- [WebSearchDiagnosticsResponse](#websearchdiagnosticsresponse)
- [WebSearchProviderStatsDto](#websearchproviderstatsdto)

---

## WebSearchDiagnosticsResponse

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

Represents the payload returned by GET /diagnostics/web-search: a set of per-provider search statistics observed over a recent fixed-size event window, a convenience flag indicating whether any provider is considered unhealthy, and the size of the event window used to compute the stats.

## Remarks
This record is a simple DTO used to transfer aggregated diagnostic metrics from the server to the UI. The HasUnhealthyProvider flag lets the UI show a single, lightweight indicator (for example a badge or warning) without inspecting each provider entry, while WindowSize allows the UI to explain the scope of the metrics ("based on the last N search calls"). Providers includes one entry for each provider that recorded events within that window.

## Example
```csharp
[HttpGet("/diagnostics/web-search")]
public ActionResult<WebSearchDiagnosticsResponse> Get()
{
    // Compute or fetch provider stats elsewhere
    IReadOnlyList<WebSearchProviderStatsDto> providers = /* compute stats */;

    // true when any provider's most recent outcome was a failure (or it had no successful calls in the window)
    bool hasUnhealthy = /* evaluate provider outcomes */;

    int windowSize = 200; // e.g. "last 200 events"

    var response = new WebSearchDiagnosticsResponse(providers, hasUnhealthy, windowSize);
    return Ok(response);
}
```

## Notes
- Providers will contain one entry per provider that recorded events in the specified window; it may be empty if no events were recorded.
- HasUnhealthyProvider uses the most-recent outcome semantics described above (it is true if any provider's most recent outcome was a failure or the provider recorded no successful calls in the window).
- WindowSize is a count of events (most-recent-N) used to compute the stats, not a time duration.
- The record and its IReadOnlyList properties are immutable from the consumer's perspective, but the underlying collection or DTO elements may still be mutable — avoid assuming deep immutability if callers can retain references.

---

## WebSearchProviderStatsDto

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

```csharp
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
    bool IsUnhealthy)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `// Display name from the System column - "web_search.<provider>" stripped
    // of the prefix (e.g. "tavily", "brave", "duckduckgo").
    string` | — |
| `TotalCalls` | `long` | — |
| `SuccessfulCalls` | `long` | — |
| `ErrorCalls` | `long` | — |
| `EmptyCalls` | `long` | — |
| `LastSuccessAt` | `DateTimeOffset?` | — |
| `LastFailureAt` | `DateTimeOffset?` | — |
| `LastFailureQuery` | `string?` | — |
| `LastFailureMessage` | `string?` | — |
| `AvgLatencyMs` | `double` | — |
| `calls` | `// True when the provider has recorded events in the window and either
    // has zero successful` | — |
| `IsUnhealthy` | `or its most recent event was a failure.
    bool` | — |


Aggregated diagnostics for a single web-search provider: counts (total, successful, errors, empty), timestamps of last success/failure, last failure context (query and message), average latency in milliseconds, and a simple health indicator. Use this DTO when returning per-provider telemetry from diagnostics endpoints or serializing provider-level observability data.

## Remarks
Provides a compact, UI-friendly summary intended for troubleshooting and monitoring. The Provider value is the display name derived from the system column (originally "web_search.<provider>") with the "web_search." prefix removed so callers see the provider identifier (e.g. "duckduckgo"). This DTO is an aggregation of event data for a time window rather than a raw event record.

## Example
```csharp
// Constructing a stats DTO after aggregating events for a provider.
var stats = new WebSearchProviderStatsDto(
    Provider: "duckduckgo",
    TotalCalls: 1243,
    SuccessfulCalls: 1189,
    ErrorCalls: 40,
    EmptyCalls: 14,
    LastSuccessAt: DateTimeOffset.UtcNow.AddMinutes(-5),
    LastFailureAt: DateTimeOffset.UtcNow.AddHours(-2),
    LastFailureQuery: "how to write docs in C#",
    LastFailureMessage: "Timeout while contacting provider API",
    AvgLatencyMs: 238.7,
    IsUnhealthy: false
);
```

## Notes
- Provider normalization: the DTO expects the provider name with the "web_search." prefix removed (e.g. "tavily", "brave", "duckduckgo").
- Nullable fields: LastSuccessAt, LastFailureAt, LastFailureQuery, and LastFailureMessage may be null when no corresponding events exist; callers should handle nulls when rendering or deriving metrics.
- AvgLatencyMs is in milliseconds (double); convert units before combining with differently measured values.
- IsUnhealthy is a heuristic: true only when the provider recorded events in the window and either has zero successful calls or its most recent event was a failure. It signals a condition worth investigating, not an absolute guarantee the provider is down.
- Privacy: LastFailureQuery and LastFailureMessage may contain user or query content—redact or truncate before exposing in public telemetry or high-volume logs as appropriate.

---
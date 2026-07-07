# WebSearchDiagnosticsResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`

## Contents

- [WebSearchDiagnosticsResponse](#websearchdiagnosticsresponse)
- [WebSearchProviderStatsDto](#websearchproviderstatsdto)

---

## WebSearchDiagnosticsResponse
> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

```csharp
public sealed record WebSearchDiagnosticsResponse(
    IReadOnlyList<WebSearchProviderStatsDto> Providers,
    
    
    
    bool HasUnhealthyProvider,
    
    
    
    int WindowSize)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Providers` | `IReadOnlyList<WebSearchProviderStatsDto>` | — |
| `HasUnhealthyProvider` | `bool` | — |
| `WindowSize` | `int` | — |


Represents the response payload for the Web Search Diagnostics endpoint. It aggregates per-provider stats recorded within the requested lookback window, exposing one entry for each provider that logged events in that window. The Providers property contains the sequence of WebSearchProviderStatsDto objects describing the stat details per provider. The HasUnhealthyProvider flag serves as a UI hint: it is true when any provider's most recent outcome was a failure, or when there have been no successful calls in the window, which prompts the UI to display a warning badge. The WindowSize property communicates the number of recent events used to compute the stats, enabling the UI to phrase results like “based on the last 200 search calls.”

## Remarks
Architecturally, WebSearchDiagnosticsResponse acts as a stable transport contract between the diagnostics data gatherer and the presentation layer. It bundles per-provider metrics with a derived health signal so the UI can convey risk without re-reading raw logs. Because it is a record, it provides value-type semantics that simplify equality comparisons, caching, and change detection across API boundaries.

## Example
```csharp
var response = new WebSearchDiagnosticsResponse(
    Providers: new List<WebSearchProviderStatsDto> { /* provider stats here */ },
    HasUnhealthyProvider: true,
    WindowSize: 200
);
```

## Notes
- The HasUnhealthyProvider flag is a UI cue derived from recent provider outcomes and is not a guaranteed holistic health assessment.
- The WindowSize communicates the scope of the statistics; ensure it matches the interpretation presented to users (e.g., "based on the last N search calls").

---

## WebSearchProviderStatsDto
> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

```csharp
public sealed record WebSearchProviderStatsDto(
    
    
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
    
    
    bool IsUnhealthy)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Provider` | `string` | — |
| `TotalCalls` | `long` | — |
| `SuccessfulCalls` | `long` | — |
| `ErrorCalls` | `long` | — |
| `EmptyCalls` | `long` | — |
| `LastSuccessAt` | `DateTimeOffset?` | — |
| `LastFailureAt` | `DateTimeOffset?` | — |
| `LastFailureQuery` | `string?` | — |
| `LastFailureMessage` | `string?` | — |
| `AvgLatencyMs` | `double` | — |
| `IsUnhealthy` | `bool` | — |


Sealed record WebSearchProviderStatsDto serves as an immutable snapshot of a single web search provider's diagnostics data. It captures the provider’s display name (derived from the System column with the "web_search." prefix stripped), usage statistics (TotalCalls, SuccessfulCalls, ErrorCalls, EmptyCalls), optional timestamps for the last success and last failure, optional context for the last failure (LastFailureQuery and LastFailureMessage), the provider’s average latency in milliseconds (AvgLatencyMs), and a flag indicating health status within the monitored window (IsUnhealthy). This object is intended for transport to diagnostics endpoints and UI components that monitor provider health and performance, rather than for in-depth per-call logic.


---
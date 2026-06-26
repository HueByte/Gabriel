# WebSearchDiagnosticsResponse.cs

> **Source:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`

## Contents

- [WebSearchDiagnosticsResponse](#websearchdiagnosticsresponse)
- [WebSearchProviderStatsDto](#websearchproviderstatsdto)

---

## WebSearchDiagnosticsResponse

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

Represents the response body for GET /diagnostics/web-search: a compact, UI-friendly summary of recent web-search metric events broken down by provider. Use this record when returning diagnostics information to clients or the UI that need a per-provider summary, a quick unhealthy indicator, and the number of most-recent events used to compute the stats.

## Remarks
This record groups WebSearchProviderStatsDto entries (one per provider that recorded events in the configured lookback window) and exposes two convenience values: HasUnhealthyProvider is a precomputed boolean the UI can use to badge the search tool when at least one provider appears unhealthy; WindowSize communicates the size of the "most recent N events" window used to compute the statistics so the UI can explain the basis of the summary.

## Example
```csharp
// Assemble a response for the diagnostics endpoint
var response = new WebSearchDiagnosticsResponse(
    Providers: new List<WebSearchProviderStatsDto> { providerAStats, providerBStats },
    HasUnhealthyProvider: true,
    WindowSize: 200);
```

## Notes
- HasUnhealthyProvider is a convenience flag; inspect Providers for provider-level detail and exact failure reasons.
- WindowSize is a count of events (the most-recent-N events used), not a time duration. Handle the case where Providers may be empty in the consumer UI or client code.

---

## WebSearchProviderStatsDto

> **File:** `src/api/Gabriel.API/Contracts/Diagnostics/WebSearchDiagnosticsResponse.cs`  
> **Kind:** record

Represents aggregated diagnostics for a single web-search provider over a reporting window. Use this DTO when returning per-provider metrics (counts, recent success/failure timestamps, last failure details, and average latency) from diagnostic APIs or health endpoints to help surface provider health and troubleshooting information.

## Remarks
This record is a compact snapshot intended for diagnostics and UI displays. It summarizes event counts observed in the reporting window, the most recent success/failure times, the last failure's query and message (if any), and average latency in milliseconds. The IsUnhealthy flag is a convenience computed from events in the same window and indicates a provider that either recorded only failures or whose latest event was a failure.

## Example
```csharp
// Constructing a sample diagnostics entry for a provider
var stats = new WebSearchProviderStatsDto(
    Provider: "duckduckgo",
    TotalCalls: 1200,
    SuccessfulCalls: 1180,
    ErrorCalls: 15,
    EmptyCalls: 5,
    LastSuccessAt: DateTimeOffset.UtcNow.AddMinutes(-10),
    LastFailureAt: DateTimeOffset.UtcNow.AddHours(-2),
    LastFailureQuery: "example search",
    LastFailureMessage: "Timeout connecting to upstream",
    AvgLatencyMs: 85.3,
    IsUnhealthy: false
);
// This object can be returned from a diagnostics endpoint or serialized to JSON for a dashboard.
```

## Notes
- LastSuccessAt and LastFailureAt are nullable: absence means no matching event was observed in the reporting window.
- LastFailureQuery may contain user-supplied search text; take care when logging or displaying it to avoid exposing sensitive data.
- AvgLatencyMs is expressed in milliseconds as a double; do not assume it is an integer or rounded value.

---
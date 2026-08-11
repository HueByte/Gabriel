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


Represents the aggregated diagnostics for the web-search feature over a defined lookback window. The Providers collection contains one entry per provider that logged events during that window, exposing provider-level statistics used by the UI to render trends and health cues. The HasUnhealthyProvider flag signals whether any provider's most recent outcome was a failure or there were no successful calls in the window, enabling a prominent warning indicator. WindowSize records the span of recent events the stats were computed over, so UI messages can say the results are based on the last 200 search calls.

## Remarks
WebSearchDiagnosticsResponse serves as a boundary object between the diagnostics API and the UI, encapsulating both granular provider stats and a high-level health signal. By aggregating per-provider data and a global unhealthy indicator, it supports both detailed rendering and quick assessments of overall health. The inclusion of WindowSize ensures users understand the timeframe of the metrics and helps prevent misinterpretation of stale data. This abstraction can accommodate additional providers or metrics without forcing UI changes.

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


WebSearchProviderStatsDto is an immutable data transfer object that aggregates per-provider web search telemetry. It captures counts (TotalCalls, SuccessfulCalls, ErrorCalls, EmptyCalls), timing information (LastSuccessAt, LastFailureAt), context for failures (LastFailureQuery, LastFailureMessage), a latency metric (AvgLatencyMs) and a health flag (IsUnhealthy) so diagnostics consumers can present a concise picture of a provider's runtime behavior.

Developers use this DTO when exporting or returning per-provider statistics from diagnostics endpoints or monitoring dashboards. It avoids scattering raw telemetry across several types and provides a stable contract for clients consuming provider health data.

## Remarks
WebSearchProviderStatsDto centralizes provider-level statistics behind a stable API surface, enabling clients to query provider health and performance without digging into granular telemetry.
Its IsUnhealthy flag encodes a simple health policy: if the provider has recorded events in the monitoring window and either has zero successful calls or the most recent event was a failure, the provider is flagged as unhealthy.
It uses a record type to provide immutable, value-based equality, making it a natural fit for API contracts and caching scenarios.

## Example
```csharp
var stats = new WebSearchProviderStatsDto(
    Provider: "tavily",
    TotalCalls: 150,
    SuccessfulCalls: 140,
    ErrorCalls: 5,
    EmptyCalls: 5,
    LastSuccessAt: DateTimeOffset.UtcNow.AddMinutes(-5),
    LastFailureAt: DateTimeOffset.UtcNow.AddMinutes(-1),
    LastFailureQuery: "weather forecast",
    LastFailureMessage: "timeout",
    AvgLatencyMs: 128.3,
    IsUnhealthy: false
);
```

---
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


WebSearchDiagnosticsResponse is the response payload for the GET /diagnostics/web-search endpoint. It wires together, in a single, immutable shape, one entry per provider that has recorded events in the requested lookback window, along with a global health flag and the window size used to compute the metrics.

## Remarks
This is an immutable transport contract represented as a sealed record, ensuring the data cannot be mutated after creation. Providers is a read-only collection of per-provider statistics, while HasUnhealthyProvider provides a concise UI signal to indicate that at least one provider failed recently (or had no successful calls within the window), which the UI can use to show a warning indicator. WindowSize communicates the scope of the statistics (the number of recent events) so users understand the recency and volume of the data.

## Notes
- The HasUnhealthyProvider flag encodes both a failure in the most recent outcome and the absence of any successful calls within the lookback window; consumers should treat it as a global health cue for the diagnostics view.
- This type is designed for network serialization; preserve the property names Providers, HasUnhealthyProvider, and WindowSize to ensure compatibility across clients and servers.


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


WebSearchProviderStatsDto is a diagnostic data transfer object that aggregates a provider's usage and health for web search diagnostics. It surfaces the provider name, throughput metrics (TotalCalls, SuccessfulCalls, ErrorCalls, EmptyCalls), recency of activity (LastSuccessAt, LastFailureAt), failure context (LastFailureQuery, LastFailureMessage), performance (AvgLatencyMs), and a derived health indicator (IsUnhealthy). Use this DTO when returning per-provider diagnostics to dashboards, alerts, or analytics so that consumers can compare reliability and latency across providers and surface quick health signals.

## Remarks
IsUnhealthy serves as a high-level health signal derived from recent activity for the provider: if there were any events in the observed window and either there are zero successful calls or the most recent event was a failure, the provider is considered unhealthy. This enables UI dashboards and alerting logic to flag problematic providers without re-computing state from individual counters.

## Notes
- IsUnhealthy is meaningful only when there have been events in the observed window; if TotalCalls == 0, the health flag reflects no activity to judge. Use TotalCalls in conjunction with IsUnhealthy if you need stricter semantics.
- LastFailureQuery and LastFailureMessage may be null; guard against nulls when displaying or logging.
- LastSuccessAt and LastFailureAt are optional, so handle their absence when deriving trends or rendering timelines.
- DateTimeOffset semantics imply awareness of time zones and offsets when presenting or persisting data.


---
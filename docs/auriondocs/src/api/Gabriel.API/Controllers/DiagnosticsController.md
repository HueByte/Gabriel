# DiagnosticsController

> **File:** `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`  
> **Kind:** class

Provides read-only operational diagnostics over the generic metric event log, surfaced as authenticated API endpoints. Use this controller when you need a quick health/usage snapshot of the web_search tool providers (aggregated from the metric log) instead of querying the metric store directly or building custom aggregation logic.

## Remarks
This controller is intentionally read-only and not admin-gated: it returns non-sensitive, aggregated telemetry that helps any authenticated user verify whether search tooling is functioning. It relies on an injected IMetricRepository to fetch recent metric rows (ordered newest-first) and performs server-side aggregation (counts, last success/failure timestamps, average latency) by provider/system prefix.

## Example
```csharp
// Request the default 200-row window for web_search diagnostics
using var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<token>");
var resp = await client.GetAsync("https://api.example.com/diagnostics/web-search?windowSize=200");
resp.EnsureSuccessStatusCode();
var body = await resp.Content.ReadAsStringAsync();
// body deserializes to the server's WebSearchDiagnosticsResponse DTO
```

## Notes
- The windowSize query parameter is clamped to the range 10..5000; requesting values outside that range will be adjusted server-side.
- Metrics with outcome "empty" are treated as successful network calls (they increment the success count and contribute to latency averages), while "error" increments the error count and records first-seen failure details.
- Rows returned from the repository are processed in newest-first order; the controller uses first-seen semantics to populate "most recent" fields (so the first row in a group is considered the most recent).
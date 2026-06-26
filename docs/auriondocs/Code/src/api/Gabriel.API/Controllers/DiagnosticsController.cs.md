# DiagnosticsController

> **File:** `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`  
> **Kind:** class

Exposes read-only operational diagnostics derived from the application's metric event log. The controller is an authenticated API surface (not admin-restricted) intended for users or tooling to verify whether services — notably the `web_search` provider — are functioning: it queries recent metric rows, aggregates outcomes (success, empty, error), computes latencies and last-seen timestamps, and returns provider-level summaries.

## Remarks
This controller intentionally performs aggregation on the read side by querying the metric repository with a system prefix (e.g., "web_search.") and a bounded window. That design keeps event ingestion simple (write-only) and lets callers request different lookback windows for short- or long-term views. The endpoints parse per-row JSON lazily and only inspect a few well-known fields (outcome, latency_ms, query, error_message), which reduces coupling to the full metric schema while still producing actionable health signals.

## Example
```csharp
// Request a 500-row snapshot of web-search diagnostics and deserialize the response.
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "<token>");
var resp = await client.GetAsync("diagnostics/web-search?windowSize=500");
resp.EnsureSuccessStatusCode();
var payload = await resp.Content.ReadFromJsonAsync<WebSearchDiagnosticsResponse>();
// payload now contains provider-level stats such as success/error counts, avg latency, and last failure info.
```

## Notes
- The controller requires authentication; callers must present valid credentials even though the surface is not admin-gated.
- The windowSize query parameter is clamped (10–5000). Results reflect only the most recent rows returned by the repository (rows are enumerated newest-first).
- Per-row metric JSON is parsed with JsonDocument. Malformed metric payloads could throw during parsing and surface as an error for the entire request.
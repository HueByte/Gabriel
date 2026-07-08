# DiagnosticsController

> **File:** `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
```


DiagnosticsController is an authenticated, read-only ASP.NET Core API controller that surfaces aggregated health and performance insights from the generic metric event log for the web_search provider. Developers reach for it when they need a stable, non-sensitive snapshot of web_search activity, latency, and outcomes without querying the raw event store directly; the endpoint is GET /diagnostics/web-search and accepts an optional windowSize to widen or narrow the time window.

## Remarks

This controller acts as a focused health-insight façade over the metric store. It aggregates recent log rows by system prefix (web_search.) into per-provider statistics, counting total invocations, successes, errors, and latency, and it records the most recent success or failure details. By performing JSON parsing lazily per-row and selecting only the fields needed for the summary, it minimizes per-call overhead while keeping the surface useful for operators and tooling. It is accessible to any authenticated user (not admin-only) and provides read-only diagnostics; it relies on the WebSearchSystemPrefix constant to filter metrics and can be extended to additional prefixes with minimal changes.
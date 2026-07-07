# DiagnosticsController

> **File:** `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
```


DiagnosticsController provides operational, read-only diagnostics over the generic metric event log and is accessible to any authenticated user (not admin-gated) so tooling stakeholders can verify that their tooling is functioning without exposing sensitive data.
The WebSearch endpoint pulls the most recent metrics prefixed with web_search. and aggregates them per provider to surface concise health statistics such as total calls, successes, errors, and latency.

## Remarks
By delegating data access to IMetricRepository, the controller keeps data access concerns isolated and the surface easy to test. The per-provider aggregation supports a scalable health view across multiple providers, and the windowSize parameter guards performance while allowing callers to tailor the time horizon. The implementation also uses lazy JSON parsing per-row to touch only the fields needed for the report, reducing allocation cost in hot paths.

## Notes
- windowSize is clamped to the range [10, 5000] to prevent abuse and control response time.
- The aggregation relies on the source order (newest first) to determine the most recent outcomes; changes in ordering could affect last-success/last-failure values.
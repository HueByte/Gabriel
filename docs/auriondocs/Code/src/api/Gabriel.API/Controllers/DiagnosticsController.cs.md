# DiagnosticsController

> **File:** `src/api/Gabriel.API/Controllers/DiagnosticsController.cs`  
> **Kind:** class

```csharp
[ApiController]
[Authorize]
[Route("diagnostics")]
public class DiagnosticsController : ControllerBase
```


DiagnosticsController provides a read-only diagnostics surface over the generic metric event log. Its WebSearch endpoint returns a per-provider health snapshot for the web_search metrics by aggregating the most recent entries and exposing key counts and latency, so developers and operators can quickly verify that their web-search tooling is functioning without exposing sensitive data.

## Remarks
The endpoint pulls the latest metrics with the "web_search." prefix, groups results by system name, and computes per-provider aggregates (total requests, successes, errors, and empty results, plus latency). Per-row JSON is parsed lazily, touching only the fields needed for the stats. A "empty" outcome is treated as a successful call with latency included. The windowSize parameter governs how many recent rows are considered and is clamped to 10–5000 to protect against large reads; use a larger window for longer-tail views if needed.

## Notes
- windowSize is clamped to 10–5000; outside values are coerced.
- Only metrics with the "web_search." prefix are included in the snapshot.
- Latency and failure data are read from the metric JSON when present; missing fields are tolerated.
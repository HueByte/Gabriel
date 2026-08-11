# InstrumentedWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class InstrumentedWebSearch : IWebSearch
```


InstrumentedWebSearch is a decorator around IWebSearch that records a metric row for every call to SearchAsync. It wraps each provider when registered so per-provider telemetry is captured even in composite search setups, and it continues to support single-provider configurations.

The payload contains the outcome, the query, the number of results, latency, and any error message. It uses a canonical system name in the form web_search.{provider} (lowercased) to ensure stable aggregation regardless of DI registration casing. On success, the outcome is "success" when results are returned, or "empty" if no results are produced. On failure, the outcome is "error" and the message from the thrown exception is recorded. Cancellations are not logged as failures; the exception is rethrown so the caller can handle it, while the decorator still emits a meaningful telemetry row for the attempted call. When recording a metric, the payload fields map to snake_case keys in the stored JSON (outcome, query, result_count, latency_ms, error_message).

## Remarks
InstrumentedWebSearch cleanly separates telemetry concerns from search logic by implementing IWebSearch and delegating actual work to the wrapped provider. This keeps observability concerns centralized and consistent across providers, while preserving the existing error handling semantics (exceptions propagate to callers, and metrics are persisted regardless). The decorator pattern here enables rich operational insight without requiring changes to individual providers or to composite orchestration code.

## Notes
- Cancellation is not treated as a provider failure; the operation is rethrown and no metric entry is created for the cancellation path.
- In error paths, the metric is recorded with CancellationToken.None to ensure the failure persists even if the original token is canceled, then the exception is rethrown to the caller.
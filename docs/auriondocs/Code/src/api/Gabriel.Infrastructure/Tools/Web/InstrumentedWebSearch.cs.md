# InstrumentedWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class InstrumentedWebSearch : IWebSearch
```


InstrumentedWebSearch is a decorator over an IWebSearch that records every search invocation as a row in the generic metrics log. It wraps the underlying provider at registration so per-provider telemetry is collected even in composite paths, and single-provider setups are tracked as well. Each call to SearchAsync is timed with a Stopwatch; on success it logs Outcome "success" along with the query and number of results, and on empty results it records "empty". If the inner call throws, it logs an "error" with the exception message and latency, then rethrows; cancellation is treated as a control signal and is not logged, preserving the original cancellation behavior. The system name is canonicalized as web_search.<provider>, lowercased to ensure stable aggregation. The payload is a WebSearchEvent, whose properties map to snake_case JSON keys via the PropertyNamingPolicy on the metric, enabling consistent analytics.
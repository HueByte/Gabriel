# InstrumentedWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`  
> **Kind:** class

```csharp
public sealed class InstrumentedWebSearch : IWebSearch
```


InstrumentedWebSearch wraps an IWebSearch and records a telemetry row for each SearchAsync call. It constructs a canonical system name of the form web_search.{provider} (lowercased) and uses IMetricRecorder to persist a WebSearchEvent containing the outcome, query, result count, latency in milliseconds, and an optional error message. The decorator times the inner call with a stopwatch, records the event, and then returns the results. If the inner call throws, it records an error event with the exception message and rethrows; cancellation is treated specially and not logged. This instrumentation is applied at registration time so every provider in a composite path (and in single-provider scenarios) yields observable, provider-level telemetry. The diagnostics endpoint uses these rows to aggregate throughput, latency, and reliability across providers.

## Remarks
InstrumentedWebSearch decouples telemetry from business logic, ensuring that provider performance and failures can be observed without modifying the underlying search implementations. By normalizing the system name and emitting structured payloads, it enables consistent, cross-provider dashboards and diagnostics even when providers are composed or swapped behind a single interface. Importantly, it preserves normal exception semantics: callers see the same exceptions as if no instrumentation were present, while telemetry is emitted in parallel.

## Notes
- Latency is measured from the moment SearchAsync is invoked until the inner call completes, excluding the time spent emitting metrics (the Stop is invoked before awaiting the metric write).
- Cancellation (OperationCanceledException) is rethrown without recording a metric event.
- On errors, the payload includes the exception message; the event is recorded with Outcome set to "error" and ResultCount to 0 before rethrowing.
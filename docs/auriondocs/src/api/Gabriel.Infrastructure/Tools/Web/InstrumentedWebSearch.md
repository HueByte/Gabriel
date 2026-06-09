# InstrumentedWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`  
> **Kind:** class

Records web search calls and outcomes for telemetry and diagnostics. Use this decorator when you want per-provider metrics for every IWebSearch invocation (success, empty, error, latency, query, and result count) without changing callers; wrap individual providers at registration so composites and single-provider setups both emit the same event shape.

## Remarks
This class is a thin decorator around an IWebSearch implementation that emits one metric row per call. It canonicalizes the metric system name to "web_search.<provider>" (lowercased) so read-side tooling can reliably aggregate across differently-cased registrations. Failures are recorded and then rethrown — the decorator does not swallow exceptions — while OperationCanceledException is treated as a caller-driven cancellation and is not logged.

## Example
```csharp
// Manual wrapping
IWebSearch provider = new SomeWebSearchProvider(...);
IMetricRecorder metrics = serviceProvider.GetRequiredService<IMetricRecorder>();
IWebSearch instrumented = new InstrumentedWebSearch(provider, metrics, "Tavily");

var results = await instrumented.SearchAsync("hello world", 10, CancellationToken.None);

// Typical DI registration (pseudo-code)
// services.AddSingleton<IWebSearch>(sp =>
//     new InstrumentedWebSearch(new SomeWebSearchProvider(...), sp.GetRequiredService<IMetricRecorder>(), "tavily"));
```

## Notes
- Cancellation (OperationCanceledException) is not recorded — the decorator treats cancellation as caller-driven and simply rethrows.
- On exceptions the metric is still recorded but using CancellationToken.None so the failure event is persisted even if the original token was already cancelled.
- The payload type uses PascalCase property names; the recorder's PropertyNamingPolicy is expected to convert them to snake_case (outcome, query, result_count, latency_ms, error_message) for storage/inspection.
- Thread-safety and lifetime follow the provided IWebSearch and IMetricRecorder implementations; this decorator itself holds only those references and a derived system name.
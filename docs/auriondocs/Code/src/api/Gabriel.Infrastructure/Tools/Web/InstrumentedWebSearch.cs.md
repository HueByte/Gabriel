# InstrumentedWebSearch

> **File:** `src/api/Gabriel.Infrastructure/Tools/Web/InstrumentedWebSearch.cs`  
> **Kind:** class

A decorator for IWebSearch that records every search invocation as a single row in the generic metric event log. Use this to wrap provider implementations (or the composite) at registration time when you want per-provider telemetry (outcome, query, result_count, latency_ms, error_message) under the stable system name web_search.<provider>.

## Remarks
This class instruments an existing IWebSearch without changing its behavior: it measures call latency with a Stopwatch, records a WebSearchEvent for every completed call (success or empty) and for failures, and then rethrows exceptions so callers see the original error. Provider names are lowercased to produce a canonical metric system name (e.g. "web_search.tavily"). The decorator intentionally omits logging OperationCanceledException (treating cancellation as caller-driven, not a provider failure) and uses CancellationToken.None when recording errors so the failure is persisted even if the original token is already cancelled.

## Example
```csharp
// Wrap a concrete provider so metrics are emitted under web_search.brave
IWebSearch inner = new BraveWebSearch(...);
IMetricRecorder recorder = serviceProvider.GetRequiredService<IMetricRecorder>();
var instrumented = new InstrumentedWebSearch(inner, recorder, "Brave");

var results = await instrumented.SearchAsync("cats", 10, CancellationToken.None);
```

## Notes
- OperationCanceledException is not recorded; cancellation is considered caller-controlled and is rethrown without a metric row. 
- The metric recording is awaited in both success and error paths — a slow or blocking IMetricRecorder will add to SearchAsync latency.
- On error the recorder is invoked with CancellationToken.None to ensure the failure is persisted even if the original token was cancelled.
- The system name is lowercased at construction; supply a provider name only (the class prefixes with "web_search.").
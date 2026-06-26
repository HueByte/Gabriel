# MetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/MetricRecorder.cs`  
> **Kind:** class

Serializes metric objects to JSON and writes them to an IMetricRepository. Intended for callers that want to record-and-forget telemetry from long-lived/singleton components; this implementation is a singleton that creates an IServiceScope per call to obtain the scoped repository.

## Remarks
This class exists to decouple metric-producing code (often singletons such as decorators, background agents or tools) from the scoped persistence implementation. It centralizes JSON serialization using a snake_case naming policy and enforces a non-fatal failure policy: serialization or persistence failures are logged at warning level and swallowed so observability does not cause runtime failures. Caller cancellation (OperationCanceledException) is propagated.

## Example
```csharp
// Typical registration in DI (conceptual):
// services.AddSingleton<IMetricRecorder, MetricRecorder>();

// Example usage from a singleton component:
public class AgentComponent
{
    private readonly IMetricRecorder _metrics;

    public AgentComponent(IMetricRecorder metrics) => _metrics = metrics;

    public async Task TickAsync(CancellationToken ct)
    {
        var payload = new { ActiveJobs = 7, Host = "worker-1" };
        // Record-and-forget semantics: failures are logged and not thrown
        await _metrics.RecordAsync("agent", payload, ct);
    }
}
```

## Notes
- Empty or whitespace `system` names are rejected: the payload is dropped and a warning is logged.
- If JSON serialization of the metric object throws, the error is logged at warning level and the metric is dropped.
- OperationCanceledException is propagated to the caller; other exceptions from persistence are logged at warning level and swallowed.
- The serializer uses a snake_case naming policy by default; apply [JsonPropertyName] on DTO properties to override names when required.
- This class is safe to register as a singleton because it creates a new IServiceScope for each RecordAsync call to obtain the scoped IMetricRepository.
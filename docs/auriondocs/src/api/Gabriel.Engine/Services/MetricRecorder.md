# MetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/MetricRecorder.cs`  
> **Kind:** class

Records metric objects for a named subsystem by serializing them to snake_case JSON and persisting via an IMetricRepository resolved from a scoped IServiceProvider. Use this singleton when you want a fire-and-forget telemetry writer that keeps observability failures from affecting application behavior.

## Remarks
MetricRecorder exists as a singleton bridge between callers (often singletons) and the scoped IMetricRepository: each RecordAsync call creates a new IServiceScope, resolves IMetricRepository, and writes a MetricEntry. Serialization uses a shared JsonSerializerOptions configured to produce snake_case JSON (individual DTOs can override property names with [JsonPropertyName]). Failures during serialization or persistence are logged at warning level and swallowed so telemetry does not disrupt the application; OperationCanceledException from an external cancellation is propagated.

## Example
```csharp
// injected via DI
private readonly IMetricRecorder _metrics;

await _metrics.RecordAsync("agents", new { id = "agent-1", state = "running", cpu = 12.3 });
```

## Notes
- If the provided system name is null, empty, or only whitespace the payload is dropped and a warning is logged.
- Serialization or repository persistence failures are logged at warning level and the metric is dropped — metrics are best-effort and may be lost.
- Caller cancellation is propagated: OperationCanceledException is rethrown and the provided CancellationToken is passed to the repository AddAsync call.
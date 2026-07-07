# MetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/MetricRecorder.cs`  
> **Kind:** class

```csharp
public sealed class MetricRecorder : IMetricRecorder
```


MetricRecorder is a singleton service that records telemetry metrics by serializing a metric payload to JSON and persisting it through a repository. It uses a per-call DI scope to resolve IMetricRepository, ensuring scoped dependencies do not leak into long-lived singletons. On RecordAsync, it validates the system name; if empty it logs a warning and drops the payload. It serializes the metric using a shared JsonSerializerOptions configured for snake_case property names, non-indented output, and ignoring null values, then persists the payload by creating a MetricEntry with the system and the JSON payload (MetricEntry.Create(system, json)) and calling AddAsync. If persistence fails, the exception is logged at warning level and swallowed so telemetry failures do not destabilize the host. If the caller cancels, the cancellation token triggers and the exception is rethrown to honor the cancellation. This design makes telemetry non-blocking for the application; metrics are collected when possible but never break the host.
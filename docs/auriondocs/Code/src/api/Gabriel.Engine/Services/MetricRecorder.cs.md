# MetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/MetricRecorder.cs`  
> **Kind:** class

```csharp
public sealed class MetricRecorder : IMetricRecorder
```


MetricRecorder is a singleton service that bridges decorators, tools, and the agent loop to the metrics store by providing a single entry point for telemetry writes. Its generic `RecordAsync<T>` method serializes a metric payload to snake_case JSON and persists it via a per-call scoped IMetricRepository; on any failure it logs a warning and drops the metric, preserving system reliability.

## Remarks

MetricRecorder centralizes the write policy for telemetry: serialization uses snake_case naming for consistent payloads, and a new DI scope is created for each write to ensure a clean lifetime for the repository. If writing fails, the system continues to operate; telemetry is observed, not allowed to crash the main flow. The class is designed for high-concurrency scenarios: the per-call scope and short-lived repository instance keep resource lifetimes tight.
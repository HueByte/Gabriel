# MetricRecorder

> **File:** `src/api/Gabriel.Engine/Services/MetricRecorder.cs`  
> **Kind:** class

```csharp
public sealed class MetricRecorder : IMetricRecorder
```


MetricRecorder is a singleton service that records metric payloads into a persistent store by serializing them to snake_case JSON and persisting them as MetricEntry records tagged with a system name. It serves as a safe, record-and-forget bridge between telemetry producers (such as decorators, tooling, and the agent loop) and the repository, ensuring scoped resources are resolved per call via a DI scope. If the system name is empty or whitespace, or if serialization fails, the payload is dropped with a warning; non-cancellation persistence failures are logged and swallowed to avoid destabilizing the host, while cancellation requests propagate to the caller.

## Remarks
This abstraction decouples metric emission from persistence concerns and centralizes the failure policy for telemetry writes. A per-call DI scope (created from IServiceScopeFactory) ensures that scoped services (like repositories or DbContexts) do not leak through the singleton. Storing the metric payload as a string JSON blob (instead of returning strong-typed metrics from the recorder) provides a flexible, schema-evolution-friendly approach while allowing subsystems to override their DTOs with JsonPropertyName attributes without affecting the recorder itself. The snake_case payload aligns with downstream querying conventions and the raw payload can be inspected in the Metric column when needed.

## Notes
- Empty or whitespace system names drop the payload with a warning.
- Serialization failures drop the payload with a warning.
- Non-cancellation persistence failures are logged and swallowed; OperationCanceledException is rethrown to propagate cancellation.
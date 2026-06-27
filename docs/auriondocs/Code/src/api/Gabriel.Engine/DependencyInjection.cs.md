# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


Configures the engine’s dependency graph by exposing AddEngineServices, an extension that wires the engine’s configuration and core services into the DI container. It binds the AgentOptions, PersonalityOptions, and AgentToolsOptions sections from configuration, registers the primary engine services (IAgentService, IToolRegistry, ITokenEstimator), the metrics surface (IMetricRecorder) and per-request context (IToolExecutionContext), and wires the system’s personality stack, prompt registry, system prompt builder, and post-processor. It also boots the Gabriel sequence: a stateless generator and a scoped service that resolves seed/state per conversation turn. Finally, it registers the full suite of tools (time, calculation, conversion, encoding, text analytics, transforms, JSON, WebSearch/WebFetch, docs, file/project tooling, and memory tools). Call this during startup to compose the engine runtime; transport concerns are handled by Gabriel.Infrastructure, while this class wires interfaces and lifetimes for the engine itself.

## Remarks
This class serves as the composition root for the Gabriel engine. It centralizes configuration, lifetime semantics, and the binding of cross-cutting concerns like metrics, prompts, and sequence resolution. While it orchestrates tool registration, the actual HTTP/transport providers live in Gabriel.Infrastructure; tool discovery relies on ToolRegistry via IEnumerable<ITool>.

## Notes
- The MetricRecorder is registered as a singleton but bridges to per-request metrics via IServiceScopeFactory, enabling safe cross-request instrumentation.
- Ensure AddEngineServices is invoked during startup before building the service provider; omissions will leave the engine unbootstrapped.
- Be mindful of lifetime interactions: many tools are registered as scoped; avoid introducing new singletons that capture scoped services.
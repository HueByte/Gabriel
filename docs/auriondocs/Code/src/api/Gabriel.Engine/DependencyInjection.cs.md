# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


Configures and wires the engine’s runtime DI graph by binding configuration options and registering core Gabriel.Engine services, tools, and per-request contexts into the application's service container. Use this extension during startup to prepare the engine for operation so controllers and services can request IAgentService, ITool, IToolRegistry, IMetricRecorder, and related collaborators via constructor injection.

## Remarks
This abstraction centralizes concerns across infrastructure and engine boundaries, ensuring a clear separation between per-request state (e.g., ToolExecutionContext) and singleton services (e.g., MetricRecorder, PromptRegistry). It also anchors the availability of the engine's tool suite by registering every built-in ITool implementation, while deferring concrete providers' wiring to Gabriel.Infrastructure.

## Notes
- Be mindful of lifetimes: most tools are registered as Scoped to align with per-turn processing.
- This extension relies on AddInfrastructure to register its provider bindings; ensure Gabriel.Infrastructure is wired up in the host application.
- Config sections AgentOptions, PersonalityOptions, and AgentToolsOptions must exist (or have sensible defaults) to avoid misconfiguration.
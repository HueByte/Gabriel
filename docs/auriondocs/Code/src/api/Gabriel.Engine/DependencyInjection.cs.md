# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


DependencyInjection is a static helper that wires the Gabriel engine’s services into the host application's DI container. AddEngineServices reads configuration sections into option objects and registers the engine's core components—agent service, tool registry, token estimator, metric recorder, per-request tool context, prompt system, Gabriel sequence, and the tool set—each with appropriate lifetimes. This centralizes all infrastructure bindings so a host can enable the engine with a single, repeatable call during startup.

## Remarks
This extension encapsulates complex DI wiring behind a single, reusable bootstrap point, reducing boilerplate and ensuring consistent discovery and lifetime boundaries for engine parts. It wires together per-turn state (IToolExecutionContext), global registries (IPromptRegistry, ISystemPromptBuilder, IResponsePostProcessor), and the tool ecosystem (ITool implementations resolved via ToolRegistry). Note that the metric recorder is registered as a singleton but writes via a scoped repository by creating a scope on each operation, so instrumented code should execute within a proper DI scope.

## Notes
- Tools are registered as scoped ITool instances, which means they are created per DI scope (e.g., per request). Consume them inside a scope.
- The invocation relies on infrastructure providers registered elsewhere (Gabriel.Infrastructure.AddInfrastructure) to supply concrete tooling like WebSearch or DocsLookup.
- This method should be called during startup to ensure all engine services are available to agents and tools.
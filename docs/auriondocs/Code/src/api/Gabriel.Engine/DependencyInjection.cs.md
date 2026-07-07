# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


DependencyInjection.AddEngineServices wires Gabriel Engine's runtime into the application's DI container. It configures option classes from the app's configuration (AgentOptions, PersonalityOptions, and AgentToolsOptions), then registers the core engine services, the tool registry, a per-request tool execution context, system prompt machinery, and a broad suite of ITool implementations. It also bridges the engine with infrastructure providers via AddInfrastructure. Call this during startup to bootstrap the engine's runtime rather than wiring each piece manually.

## Remarks
This extension is the central bootstrap point for composing the engine's service graph. It encapsulates lifetimes and composition decisions (singletons for stateless registries/builders; scoped for per-turn contexts and tools) so downstream code can rely on consistent wiring. Tool discovery is driven by the registry through `IEnumerable<ITool>` injection, so adding new tools typically requires only registering the implementation here. Some tools depend on infrastructure-backed providers, which are supplied by Gabriel.Infrastructure.DependencyInjection.AddInfrastructure; ensure that infrastructure wiring is performed as part of application startup.

## Notes
- Tools that depend on infrastructure providers will only function after Gabriel.Infrastructure.AddInfrastructure has registered its providers.
- The extension relies on discovery via ToolRegistry (IServiceCollection registrations of multiple ITool implementations) to populate the available tool set; adding new tools typically requires only new ITool registrations in this method. If you mutate option values at runtime, consider using IOptionsMonitor or IOptionsSnapshot to observe changes.
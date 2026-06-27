# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


DependencyInjection is the composition root for Gabriel.Engine. Its AddEngineServices extension wires the engine's configuration, services, and tooling into the DI container, centralizing startup bindings for agents, prompts, metrics, and tools. Use it during application startup to ensure the engine components are available to agents and tools in a consistent, testable way while infrastructure concerns are wired separately.

## Remarks
- All engine-related services are registered with appropriate lifetimes: singletons cover stateless registries and configuration-driven components, while scoped services (like IToolExecutionContext and IGabrielSequenceService) support per-turn work.
- Centralizing bindings here enforces a clear separation between engine concerns and infrastructure wiring, making it easier to swap implementations (for testing or customization) without touching call sites.
- Infrastructure providers (e.g., web search, docs lookup) are wired by Gabriel.Infrastructure.AddInfrastructure elsewhere, keeping engine wiring decoupled from infrastructure concerns.

## Example
```csharp
// Startup-level wiring
public void ConfigureServices(IServiceCollection services)
{
    services.AddEngineServices(Configuration);
}
```

## Notes
- Tools registered here are scoped; avoid capturing scoped services in singletons.
- Ensure infrastructure bindings are configured (AddInfrastructure) so tools that depend on external providers resolve correctly.
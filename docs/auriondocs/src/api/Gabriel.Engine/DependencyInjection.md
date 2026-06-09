Registers the Gabriel engine's services, tools and configuration bindings into an IServiceCollection so the engine's agent, tool registry, token estimator, metric recorder, sequence services, personality/prompt components and a set of ITool implementations are available via DI. Use this extension from application startup (Program.cs or Startup) to wire the engine surface into the host container rather than registering each service by hand.

## Remarks
This class centralizes composition for all engine-facing concerns (agent orchestration, tool discovery, prompt/personality construction, metrics and sequence generation). Network/transport-backed provider implementations (for example web search or docs lookup) live in the Infrastructure assembly and are intentionally not registered here — that separation keeps HTTP/transport concerns where they belong while this assembly declares the engine-side consumers and the concrete ITool registrations. ToolRegistry discovers all registered ITool instances via IEnumerable<ITool> injection, so adding a tool here makes it available to the registry automatically.

## Example
```csharp
// Program.cs (or Startup.cs) — typical usage
var builder = WebApplication.CreateBuilder(args);
// Ensure infrastructure-side providers are registered somewhere (e.g. Gabriel.Infrastructure)
// builder.Services.AddInfrastructure(builder.Configuration);

// Wire Gabriel.Engine components and tools
builder.Services.AddEngineServices(builder.Configuration);

var app = builder.Build();
// ...
```

## Notes
- The method binds AgentOptions, PersonalityOptions and AgentToolsOptions from configuration using each type's SectionName constant; missing sections result in default option values.
- Many services are registered as scoped (per-request) while a few diagnostic/prompt components are singletons — follow the declared lifetimes when adding collaborators to avoid captive dependency issues.
- IMetricRecorder is a singleton that delegates writes to a scoped IMetricRepository via IServiceScopeFactory; do not register IMetricRepository as a singleton or you will break the intended lifetime/behavior.
- Several tools depend on provider implementations registered in Gabriel.Infrastructure. If those providers are not registered, resolving those tools will fail at runtime.
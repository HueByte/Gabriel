Registers the Gabriel Engine's core services, configuration bindings, and built-in tools into an IServiceCollection. Use this from your application's startup (Program.cs / Startup) when you want the engine, its personality/prompt stack, metrics plumbing, sequence services, and default tool set wired into DI rather than registering each piece manually.

## Remarks
This extension centralizes engine wiring so callers don't need to know individual registrations or lifetimes. Configuration sections for AgentOptions, PersonalityOptions, and AgentToolsOptions are bound here; stateless, config-driven components are registered as singletons while per-request/turn state (tools and execution context) are scoped. All ITool implementations added here are later discovered by ToolRegistry via IEnumerable<ITool> injection. Note that infrastructure-side providers (e.g. IWebSearch, IDocsLookup) are registered in Gabriel.Infrastructure; ensure those are added to the IServiceCollection as well so tools depending on them have their dependencies satisfied.

## Example
```csharp
// Program.cs (minimal example)
var builder = WebApplication.CreateBuilder(args);
// register infrastructure providers first (from Gabriel.Infrastructure)
builder.Services.AddInfrastructure(builder.Configuration);
// register engine services and default tools
builder.Services.AddEngineServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

## Notes
- MetricRecorder is registered as a singleton but is intended to bridge to a scoped IMetricRepository via IServiceScopeFactory; the repository itself must be registered with a scoped lifetime.
- IToolExecutionContext is scoped and must be populated (AgentService.Set) once per agent turn — tools expect the context to be available at execution time.
- Some tools registered here depend on providers added by Gabriel.Infrastructure.AddInfrastructure; failing to add those infrastructure registrations will cause runtime DI resolution errors.
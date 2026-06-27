Registers Gabriel engine components, configuration-backed options, and all built-in tools into an IServiceCollection. Use this extension from your application's startup (Program.cs / Startup.cs) to wire the agent core, personality stack, tool implementations, metrics, token estimator, and sequence services so the Gabriel engine can run.

## Remarks
This static class centralizes DI registrations for the engine layer so callers don't need to know individual service lifetimes or implementation types. It intentionally registers pure/config-driven components as singletons (prompt registry, system prompt builder, post-processor), per-request state as scoped (agent service, tool execution context, sequence service), and shared utilities as singletons (metric recorder, token estimator). Tool implementations are registered as ITool implementations here and are discovered by ToolRegistry via IEnumerable<ITool> constructor injection. Providers that perform HTTP/transport work (e.g. external model or search providers) are registered in the Infrastructure layer — call that registration before or alongside AddEngineServices.

## Example
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register infrastructure providers first (hosts, HTTP clients, external providers)
builder.Services.AddInfrastructure(builder.Configuration);

// Then register Gabriel engine services and built-in tools
builder.Services.AddEngineServices(builder.Configuration);

var app = builder.Build();
```

## Notes
- AddInfrastructure (from Gabriel.Infrastructure) must be called so tools depending on infra providers (IWebSearch, IDocsLookup, etc.) have their dependencies satisfied.
- Each ITool added here is picked up automatically by ToolRegistry; when adding custom tools, register them as ITool so they are discovered.
- MetricRecorder is a singleton write surface that bridges to a scoped IMetricRepository via IServiceScopeFactory; ensure a concrete IMetricRepository is registered in DI if diagnostics/reads are required.
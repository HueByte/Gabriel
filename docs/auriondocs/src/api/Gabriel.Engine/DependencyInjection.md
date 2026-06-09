# DependencyInjection

> **File:** `src/api/Gabriel.Engine/DependencyInjection.cs`  
> **Kind:** class

Registers Gabriel Engine services, tools and configuration into an IServiceCollection. Use this extension from application startup (Program.cs / Startup.cs) to wire the engine's agent, tooling, metric and prompt subsystems instead of registering each implementation manually.

## Remarks
This is the central DI composition root for the Gabriel engine surface: it binds options, core agent services, the prompt/personality stack, the Gabriel sequence components, metric recording, path resolution and every ITool implementation the engine exposes. Lifetime choices are deliberate — stateless/config-driven components are singletons, per-request concerns and tool execution contexts are scoped — and ToolRegistry discovers tools by consuming `IEnumerable<ITool>`, so adding each ITool here is how new tools are made available to the engine. Provider implementations that perform HTTP/transport (for example Grok or any external search/docs provider) are registered in Gabriel.Infrastructure; call that registration as well.

## Example
```csharp
// In Program.cs (minimal):
var builder = WebApplication.CreateBuilder(args);

// register infrastructure providers first (search, docs providers, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// register engine services, tools and options
builder.Services.AddEngineServices(builder.Configuration);

var app = builder.Build();
app.MapControllers();
app.Run();
```

## Notes
- MetricRecorder is registered as a singleton but bridges to scoped IMetricRepository via IServiceScopeFactory; it must not directly depend on scoped services.
- Tools that depend on infrastructure-side providers (IWebSearch, IDocsLookup, etc.) require Gabriel.Infrastructure registrations — missing those will cause runtime DI failures.
- Tool discovery uses `IEnumerable<ITool>` injection; registering additional ITool implementations in this method is how they become available to the ToolRegistry.
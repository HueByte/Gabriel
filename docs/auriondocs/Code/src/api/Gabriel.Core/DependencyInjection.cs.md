# DependencyInjection

> **File:** `src/api/Gabriel.Core/DependencyInjection.cs`  
> **Kind:** class

Registers the core Gabriel domain services (IChatService, IProjectService, IMemoryService) into an IServiceCollection so they can be resolved by the application's dependency injection container. Use this extension in application startup when you want the domain-level services wired into DI without pulling in the agent/engine stack (which is registered separately via AddEngineServices).

## Remarks
This class provides a small, focused composition root for domain wiring — it keeps Gabriel's domain service registrations separate from the engine/agent registrations (the latter live in Gabriel.Engine and are added with AddEngineServices). Each service is registered with a scoped lifetime, making them appropriate for typical per-request lifetimes in web or hosted applications.

## Example
```csharp
// Program.cs (minimal hosting)
var builder = WebApplication.CreateBuilder(args);

// Register core domain services
builder.Services.AddCoreServices();

// Register engine/agent stack (separate package)
builder.Services.AddEngineServices();

var app = builder.Build();
app.Run();
```

## Notes
- The services are registered with AddScoped; do not capture these scoped services inside singletons or resolve them from the root service provider.
- This helper only wires domain services. Engine-specific registrations and the agent loop live in Gabriel.Engine and must be registered separately via AddEngineServices.
- Avoid calling AddCoreServices multiple times to prevent duplicate registrations for the same service types.
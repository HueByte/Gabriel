# DependencyInjection

> **File:** `src/api/Gabriel.Core/DependencyInjection.cs`  
> **Kind:** class

Registers the core domain services used by the Gabriel application into an IServiceCollection. Use this extension from your application's startup (or Program.cs) to compose the domain-level services (chat, project, memory) into the DI container; engine-level wiring (LLM providers, tools, ReAct loop, etc.) is intentionally registered separately via AddEngineServices.

## Remarks
This class provides a small composition root for domain concerns so that service registrations stay grouped and discoverable. It keeps core service wiring separate from the engine wiring (which lives in Gabriel.Engine and is registered with AddEngineServices), making it easier to reuse or test the domain components independently from the engine stack.

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

// Register domain/core services
builder.Services.AddCoreServices();

// Register engine-level services (providers, tools, ReAct loop)
builder.Services.AddEngineServices(builder.Configuration);

var app = builder.Build();
// ...
```

## Notes
- The services are registered with a Scoped lifetime (AddScoped). Account for that when resolving from singletons or background threads.
- If you need to replace any default implementation, register your replacement after calling AddCoreServices (in ASP.NET Core the last registration wins for single-service resolution).
- Calling AddCoreServices multiple times will add additional descriptors for the same service types; prefer a single call during application startup.
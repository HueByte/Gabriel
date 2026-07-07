# DependencyInjection

> **File:** `src/api/Gabriel.Core/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


DependencyInjection is a domain-wiring helper that provides AddCoreServices, an extension on IServiceCollection. It registers IChatService, IProjectService, and IMemoryService to their concrete implementations as scoped services, centralizing core-domain wiring so startup code can call a single method instead of registering each service individually; engine-related setup remains separate and is wired via AddEngineServices.

## Remarks
By grouping core service registrations behind a single extension, this symbol keeps the composition root tidy and makes the intended lifetimes explicit. It also promotes testability by allowing mocks or stubs to be substituted for the interfaces in isolation, without touching startup code.

## Example
```csharp
// Common usage at startup
services.AddCoreServices();
```

## Notes
- Scoped lifetime means one instance per DI scope (per web request in ASP.NET Core); in non-web apps, create a scope to ensure per-operation instances.
- Call AddCoreServices once in the composition root to avoid confusion from multiple registrations.
- This extension wires only the core domain services; engine-related registrations are handled separately via AddEngineServices.
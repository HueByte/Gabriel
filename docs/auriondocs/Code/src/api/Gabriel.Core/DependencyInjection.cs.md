# DependencyInjection

> **File:** `src/api/Gabriel.Core/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


Registers the core Gabriel.Core domain services into the application's dependency injection container. AddCoreServices wires the concrete implementations behind the core interfaces IChatService, IProjectService, and IMemoryService (ChatService, ProjectService, MemoryService) so they resolve with a scoped lifetime. Returning the IServiceCollection enables fluent composition during startup and keeps domain wiring separate from engine-level registrations.

## Remarks
This extension centralizes the core domain registrations, reducing duplication and making it easier to swap implementations for testing. It ensures the core services are available within the request scope, promoting consistent lifetimes and predictable resolution across the application. By separating this wiring from the engine's AddEngineServices, the domain layer remains decoupled from engine concerns, allowing independent evolution and clearer layering.

## Example
```csharp
// Typical startup wiring
services.AddCoreServices();
```

## Notes
- All registrations are scoped; avoid storing scoped services in singleton components.
- Calling AddCoreServices multiple times is unnecessary; prefer a single, centralized startup call to configure core domain services.
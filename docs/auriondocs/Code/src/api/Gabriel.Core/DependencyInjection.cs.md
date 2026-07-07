# DependencyInjection

> **File:** `src/api/Gabriel.Core/DependencyInjection.cs`  
> **Kind:** class

```csharp
public static class DependencyInjection
```


Registers core domain services into the application's dependency injection container. AddCoreServices centralizes the wiring of the domain services IChatService, IProjectService, and IMemoryService to their concrete implementations (ChatService, ProjectService, MemoryService) with a scoped lifetime, making them available to consumers via constructor injection. This extension method is intended to be invoked during startup to assemble the core services for Gabriel.Core and to align with the engine wiring performed by AddEngineServices.

## Remarks
By isolating service registrations in AddCoreServices, the codebase gains a reusable and testable composition point for core domain concerns, decoupling concrete implementations from the rest of the startup logic. It complements the engine wiring by providing the domain-facing services that the engine components rely upon, while preserving a clear separation between domain wiring and engine registration.

## Notes
- The registered lifetimes are scoped; avoid capturing these services in singletons or exposing them through singleton registrations.
- This method focuses on three core services; extending core wiring should continue to be centralized here rather than scattered across startup code to maintain consistency.
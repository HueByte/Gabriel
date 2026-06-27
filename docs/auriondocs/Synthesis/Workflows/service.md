# Adding a new service

> *Workflow template auto-derived from 7 existing exemplar(s).*

Adding a new service

When you need a small, testable boundary that encapsulates domain logic, orchestrates repositories, and returns DTOs for the rest of the app, add a service. Services in this codebase live in the core services folder and follow a simple interface + implementation pattern so they are easy to register with DI and mocked in tests.

## Scaffold

```csharp
namespace YourProject.Services;

public interface IFooService
{
    Task<FooDto?> GetAsync(Guid id, CancellationToken ct);
}

public class FooService : IFooService
{
    private readonly IFooRepository _repository;
    private readonly ILogger<FooService> _logger;

    public FooService(IFooRepository repository, ILogger<FooService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<FooDto?> GetAsync(Guid id, CancellationToken ct)
    {
        var entity = await _repository.GetAsync(id, ct);
        return entity is null ? null : new FooDto(entity.Id, entity.Name);
    }
}
```

## Where it lives

Place the new interface and implementation in the core services folder used by the project: src/api/Gabriel.Core/Services. Follow the established naming convention visible in the exemplars: the interface is prefixed with I (for example IChatService.cs) and the concrete implementation uses the corresponding name with the Service suffix (for example ChatService.cs). Use the same namespace pattern as other services in that folder.

## DI wiring

Register the new service in the composition root alongside the other services (where existing services like ChatService, MemoryService, and ProjectService are registered). Add a single registration line that mirrors the existing pattern, for example:

services.AddScoped<IFooService, FooService>();

Match the service lifetime (Scoped/Singleton/Transient) to the surrounding registrations — add the line in the same file where other service registrations are performed so your new service is picked up at startup.

## Existing examples

- [ChatService.cs](Code/src/api/Gabriel.Core/Services/ChatService.cs.md)
- [IChatService.cs](Code/src/api/Gabriel.Core/Services/IChatService.cs.md)
- [IMemoryService.cs](Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [IProjectFileService.cs](Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [IProjectService.cs](Code/src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [MemoryService.cs](Code/src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [ProjectService.cs](Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)

---
*Synthesised by Aurion on 2026-06-08 22:36:41 UTC*

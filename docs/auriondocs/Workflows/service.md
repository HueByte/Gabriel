# Adding a new service

> *Workflow template auto-derived from 7 existing exemplar(s).*

# Adding a new service (csharp)

Reach for this pattern when you need a new application-domain service that encapsulates business logic and depends on a repository or other collaborators. The pattern creates a small interface plus a concrete implementation (I<Name>Service and <Name>Service) that translate repository/entities into DTOs and are registered with the app's dependency-injection container so callers can take a dependency on the interface.

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

Place the new interface and implementation alongside the other domain services in src/api/Gabriel.Core/Services. Follow the established naming convention: interface names start with I and end with Service (for example, [IChatService.cs](Code/src/api/Gabriel.Core/Services/IChatService.cs.md)), and implementations use the same base name without the leading I (for example, [ChatService.cs](Code/src/api/Gabriel.Core/Services/ChatService.cs.md), [MemoryService.cs](Code/src/api/Gabriel.Core/Services/MemoryService.cs.md), [ProjectService.cs](Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)). Keep the service focused on orchestration and DTO translation; repository access and domain entities stay behind repository interfaces.

## DI wiring

Register the new service at the application's composition root where other services are wired into the IServiceCollection. Concretely, find where the existing services such as [ChatService.cs](Code/src/api/Gabriel.Core/Services/ChatService.cs.md) or [ProjectService.cs](Code/src/api/Gabriel.Core/Services/ProjectService.cs.md) are registered and add a single line in the same place, for example:

services.AddTransient<IFooService, FooService>();

Use the same lifetime (AddTransient/AddScoped/AddSingleton) chosen for the similar services you located.

## Existing examples

- [ChatService.cs](Code/src/api/Gabriel.Core/Services/ChatService.cs.md)
- [IChatService.cs](Code/src/api/Gabriel.Core/Services/IChatService.cs.md)
- [IMemoryService.cs](Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [IProjectFileService.cs](Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [IProjectService.cs](Code/src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [MemoryService.cs](Code/src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [ProjectService.cs](Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)

---
*Synthesised by Aurion on 2026-06-09 03:25:30 UTC*

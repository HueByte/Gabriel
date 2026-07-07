# Adding a new service

> *Workflow template auto-derived from 7 existing exemplar(s).*

Adding a new service is the pattern you reach for when you need a focused business-logic component that sits between repositories and higher-level application code (controllers, handlers, or other services). This guide shows the minimal interface + implementation shape to add, where to place the files in this repository, and the single DI registration line you typically add so the new service can be injected where it’s consumed.

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

Place the new interface and implementation together under the Services folder for the core assembly. Based on the exemplars, that folder is src/api/Gabriel.Core/Services; follow the convention of naming the interface I{Name}Service (for example, IChatService) and the concrete class {Name}Service (for example, ChatService). Keep DTOs and repository interfaces referenced by the service in their respective locations so the service itself remains focused on orchestration and business logic.

## DI wiring

Register the service in the application's dependency-injection composition root with a single line that mirrors how other services are registered. The registration code to add is typically:

services.AddScoped<IFooService, FooService>();

To find the right file to edit, search the codebase for registrations of existing services such as IChatService or IProjectService and add the new line next to them in the same composition root. This ensures the service is available for constructor injection where it’s consumed.

## Existing examples

- [`ChatService`](../../Code/src/api/Gabriel.Core/Services/ChatService.cs.md)
- [`IChatService`](../../Code/src/api/Gabriel.Core/Services/IChatService.cs.md)
- [`IMemoryService`](../../Code/src/api/Gabriel.Core/Services/IMemoryService.cs.md)
- [`IProjectFileService`](../../Code/src/api/Gabriel.Core/Services/IProjectFileService.cs.md)
- [`IProjectService`](../../Code/src/api/Gabriel.Core/Services/IProjectService.cs.md)
- [`MemoryService`](../../Code/src/api/Gabriel.Core/Services/MemoryService.cs.md)
- [`ProjectService`](../../Code/src/api/Gabriel.Core/Services/ProjectService.cs.md)

---
*Synthesised by Aurion on 2026-07-07 18:13:45 UTC*

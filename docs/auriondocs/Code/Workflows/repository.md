# Adding a new repository

> *Workflow template auto-derived from 4 existing exemplar(s).*

When you need to add a new data-access surface for a domain entity — for example to encapsulate EF Core queries behind a testable interface or to keep DB concerns out of business logic — add a repository following this pattern. The scaffold below shows the minimal shape: an interface named I<Type>Repository exposing async methods, and a concrete implementation that accepts an AppDbContext and implements those methods with EF Core queries.

## Scaffold

```csharp
namespace YourProject.Repositories;

public interface IFooRepository
{
    Task<FooEntity?> GetAsync(Guid id, CancellationToken ct);
    Task<List<FooEntity>> ListAsync(CancellationToken ct);
}

public class FooRepository : IFooRepository
{
    private readonly AppDbContext _db;

    public FooRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<FooEntity?> GetAsync(Guid id, CancellationToken ct)
        => _db.Foos.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<List<FooEntity>> ListAsync(CancellationToken ct)
        => _db.Foos.OrderBy(f => f.CreatedAt).ToListAsync(ct);
}
```

## Where it lives

Repository interfaces and their implementations are colocated with other repositories under src/api/Gabriel.Core/Repositories. Follow the naming convention shown in the scaffold and exemplars: the interface is I{Name}Repository (for example, IConversationRepository) and the concrete implementation is {Name}Repository (for example, ConversationRepository or ConversationEfRepository) that depends on AppDbContext and implements the interface.

## DI wiring

Register the new repository with the application's dependency injection container so consumers receive the interface. The one-line registration you need is the standard service registration such as:

services.AddScoped<IFooRepository, FooRepository>();

Add that single line to your application's DI composition root where other repositories are registered (the place in the project responsible for wiring services into the container). This ensures the concrete FooRepository is injected wherever IFooRepository is required.

## Existing examples

- [`IConversationRepository`](../../Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [`IMemoryRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [`IMetricRepository`](../../Code/src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [`IProjectRepository`](../../Code/src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)

---
*Synthesised by Aurion on 2026-07-07 18:14:09 UTC*

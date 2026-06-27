# Adding a new repository

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new repository

When you need to encapsulate database access for a new entity (read, list, and similar query operations) add a repository following the existing project pattern. This keeps data access consistent, testable, and discoverable across the codebase.

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

Place the repository interface (I{Name}Repository) and its implementation ({Name}Repository) alongside the other repositories in src/api/Gabriel.Core/Repositories. The existing files follow an interface-first naming convention such as IConversationRepository.cs, IMemoryRepository.cs, IMetricRepository.cs, and IProjectRepository.cs; mirror that pattern so other developers can quickly find and recognize repository types.

## DI wiring

Register your new repository in the application's dependency-injection composition root where other repositories are registered by adding a single line that maps the interface to the implementation. For example: services.AddScoped<IFooRepository, FooRepository>();. To find the correct file to edit, search the codebase for an existing repository symbol (for example IConversationRepository) and add the same registration alongside the other repository registrations you find there.

## Existing examples

- [IConversationRepository.cs](Code/src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [IMemoryRepository.cs](Code/src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [IMetricRepository.cs](Code/src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [IProjectRepository.cs](Code/src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)

---
*Synthesised by Aurion on 2026-06-08 22:37:02 UTC*

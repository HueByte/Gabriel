# Adding a new repository

> *Workflow template auto-derived from 4 existing exemplar(s).*

Adding a new repository is the pattern you reach for when you need a typed abstraction over database access for a new aggregate or entity — a place to put EF Core queries and mapping logic, and to keep data access out of higher-level services. Repositories in this codebase follow a simple interface + concrete implementation shape and are discovered/registered alongside the other repository types.

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

Place repository types in the Repositories folder under the core project: src/api/Gabriel.Core/Repositories. Follow the established naming convention shown by the existing files: interface names begin with an I and end with Repository (for example, IConversationRepository.cs, IMemoryRepository.cs, IMetricRepository.cs, IProjectRepository.cs). The implementation class uses the same base name without the I prefix (e.g., ConversationRepository, MemoryRepository) and is commonly kept alongside its interface in the same folder.

## DI wiring

Register the new repository implementation with the dependency injection container using the same pattern as other repositories. Add a single service registration such as services.AddScoped<IFooRepository, FooRepository>(); at the composition/root location where repository registrations are grouped — look for existing registrations that reference IConversationRepository, IMemoryRepository, IMetricRepository, or IProjectRepository and add the analogous line next to them.

## Existing examples

- [IConversationRepository.cs](src/api/Gabriel.Core/Repositories/IConversationRepository.cs.md)
- [IMemoryRepository.cs](src/api/Gabriel.Core/Repositories/IMemoryRepository.cs.md)
- [IMetricRepository.cs](src/api/Gabriel.Core/Repositories/IMetricRepository.cs.md)
- [IProjectRepository.cs](src/api/Gabriel.Core/Repositories/IProjectRepository.cs.md)

---
*Synthesised by Aurion on 2026-06-09 03:25:57 UTC*

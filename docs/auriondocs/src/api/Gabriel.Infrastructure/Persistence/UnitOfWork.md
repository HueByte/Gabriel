# UnitOfWork

> **File:** `src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs`  
> **Kind:** class

A minimal Unit of Work implementation that exposes a single SaveChangesAsync method by delegating to an injected AppDbContext. Use this when you want an IUnitOfWork abstraction to encapsulate committing changes from higher-level services without depending directly on Entity Framework's DbContext.

## Remarks
This class is intentionally thin: it only forwards SaveChangesAsync to the underlying AppDbContext and does not manage transactions, lifetimes, or other cross-cutting concerns. It exists to provide a testable, DI-friendly abstraction (IUnitOfWork) so application services can request a commit point without taking a dependency on EF Core types.

## Example
```csharp
// Register in DI (typical ASP.NET Core setup)
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Consume in an application service
public class SomeService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepository<Foo> _repo;

    public SomeService(IUnitOfWork uow, IRepository<Foo> repo)
    {
        _uow = uow;
        _repo = repo;
    }

    public async Task CreateAsync(Foo item, CancellationToken ct)
    {
        _repo.Add(item);
        await _uow.SaveChangesAsync(ct);
    }
}
```

## Notes
- The UnitOfWork does not control DbContext lifetime; ensure AppDbContext is registered with an appropriate scope (usually scoped) in DI.
- It does not create or manage explicit database transactions — use EF Core's IDbContextTransaction or ambient transactions if you need multi-step transactional behavior.
- Exceptions from SaveChangesAsync propagate to the caller; handle DbUpdateException/DbUpdateConcurrencyException as appropriate.
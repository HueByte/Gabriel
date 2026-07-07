# UnitOfWork

> **File:** `src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs`  
> **Kind:** class

```csharp
public class UnitOfWork : IUnitOfWork
```


UnitOfWork is a minimal implementation of IUnitOfWork that wraps AppDbContext and delegates persistence to EF Core. It exposes a single asynchronous SaveChangesAsync operation, which commits all tracked changes to the database, enabling callers to coordinate multiple repository operations within a single unit of work without depending directly on the DbContext.

## Remarks
By depending on IUnitOfWork instead of the EF Core DbContext, services can be tested with mocks or stubs and the underlying persistence mechanism can be swapped in the future without touching higher layers. This class does not implement explicit transaction management; it relies on AppDbContext.SaveChangesAsync to perform the commit within the surrounding DI scope. The lifetime of the DbContext is managed by the DI container, so disposal is handled at the appropriate scope boundary.

## Notes
- No explicit transaction management is implemented here; SaveChangesAsync commits changes in a single database transaction per call (as provided by the DbContext).
- This is a thin wrapper; it does not expose repository methods or read/query capabilities. If you need richer data access patterns, introduce dedicated repositories or extend the abstraction.
- Ensure AppDbContext is registered with a compatible lifetime so the UnitOfWork and context share a consistent scope (avoiding multiple contexts or disposed instances).
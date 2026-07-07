# UnitOfWork

> **File:** `src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs`  
> **Kind:** class

```csharp
public class UnitOfWork : IUnitOfWork
```


UnitOfWork wraps an AppDbContext and implements IUnitOfWork by delegating its SaveChangesAsync method to the underlying context. It exposes a single asynchronous SaveChangesAsync method, acting as a persistence boundary that commits all tracked changes in one operation rather than scattering SaveChanges calls across repositories. Developers reach for this wrapper to depend on IUnitOfWork instead of a concrete DbContext, enabling easier testing and a cleaner separation of concerns around data access.

## Remarks
By abstracting the persistence surface behind IUnitOfWork, this class helps isolate business logic from EF Core internals and provides a natural place to add coordination or transactional behavior in one layer later. In this specific implementation, there is no explicit transaction management beyond EF Core's SaveChangesAsync; any multi-repository coordination would be implemented at a higher layer or extended here.

## Notes
- This is a thin wrapper; it does not coordinate transactions across multiple repositories by itself.
- Make sure the AppDbContext's lifetime is aligned with the UnitOfWork; sharing or disposing it unexpectedly can cause disposed-context errors.
- Pass a CancellationToken when calling SaveChangesAsync to support cancellation of long-running saves.
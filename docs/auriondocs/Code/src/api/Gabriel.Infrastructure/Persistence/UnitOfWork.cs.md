# UnitOfWork

> **File:** `src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs`  
> **Kind:** class

```csharp
public class UnitOfWork : IUnitOfWork
```


UnitOfWork is a concrete implementation of IUnitOfWork that encapsulates an AppDbContext and exposes a single asynchronous operation to persist changes. It forwards the provided CancellationToken to the underlying context's SaveChangesAsync, returning the number of state entries written to the database. This class provides a persistence contract that higher-level services can depend on, without needing to reference EF Core details directly.

## Remarks
This symbol acts as a minimal façade over the data layer, decoupling domain services from EF Core specifics and enabling easier testing and substitution of the persistence mechanism. By sharing a single context, multiple repositories can participate in a single SaveChangesAsync call, aligning with the Unit of Work pattern where a coordinated commit is desired.

## Notes
- This implementation does not introduce its own transaction scope; SaveChangesAsync commits changes tracked by the AppDbContext in one batch. If cross-context or multi-transaction behavior is required, external transaction management is needed.
- Do not instantiate UnitOfWork per repository; inject and reuse the same instance (typically with a scoped lifetime) so changes across repositories can be saved together via a single SaveChangesAsync call.
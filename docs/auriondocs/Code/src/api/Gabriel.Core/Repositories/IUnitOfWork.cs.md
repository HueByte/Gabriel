# IUnitOfWork

> **File:** `src/api/Gabriel.Core/Repositories/IUnitOfWork.cs`  
> **Kind:** interface

```csharp
public interface IUnitOfWork
```


IUnitOfWork defines a contract to coordinate persistence across repositories, enabling a single commit for all in-flight changes. Implementations are invoked by services once per use-case to persist changes as one transactional unit, improving consistency and reducing the risk of partial updates when multiple repositories are involved. The SaveChangesAsync method accepts an optional CancellationToken and returns the number of state entries written to the underlying data store.

## Remarks
By encapsulating transaction boundaries, this abstraction decouples repositories from the specifics of the persistence technology and promotes testability through mock or faked implementations. It clarifies the orchestration point where multiple aggregates are persisted together, helping maintain a consistent domain state across operations.

## Example
```csharp
// Example usage within a service that has performed repository updates
int persisted = await _unitOfWork.SaveChangesAsync(ct);
```

## Notes
- The return value represents the number of state entries written to the database; depending on the provider, this may not map one-to-one with logical operations.
- Pass a meaningful CancellationToken to support cooperative cancellation of the commit operation.
- The exact transactional guarantees depend on the concrete implementation and data provider; some implementations wrap changes in a transaction, others rely on provider-specific semantics.}
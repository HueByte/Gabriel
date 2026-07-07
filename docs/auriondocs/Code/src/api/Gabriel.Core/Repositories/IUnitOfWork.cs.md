# IUnitOfWork

> **File:** `src/api/Gabriel.Core/Repositories/IUnitOfWork.cs`  
> **Kind:** interface

```csharp
public interface IUnitOfWork
```


IUnitOfWork defines a contract for coordinating persistence across repositories by committing all in-flight changes as a single transaction. Implementations typically share a data context so that operations performed through multiple repositories can be persisted atomically. Use SaveChangesAsync at the end of a use-case to persist changes; it accepts a CancellationToken for cooperative cancellation and returns the number of state entries written to the data store.

## Remarks
Unit-of-work serves as the boundary around a business transaction that spans multiple repositories. It decouples repository operations from transaction management, ensuring a single commit point and helping maintain consistency of domain invariants. This abstraction is particularly beneficial in tests, as you can mock or stub IUnitOfWork to simulate transactional commits.

## Example
```csharp
// Typical usage: perform several repository operations, then commit once.
public async Task<int> CompleteAsync(IUnitOfWork uow, CancellationToken ct)
{
    // perform repository operations using repositories tied to the same context
    // e.g., await _customerRepo.AddAsync(customer, ct);
    // e.g., await _orderRepo.UpdateAsync(order, ct);
    return await uow.SaveChangesAsync(ct);
}
```

## Notes
- Exceptions from SaveChangesAsync indicate persistence failures; consider wrapping in a try/catch or applying a retry/transient-failure strategy as appropriate.
- The return value is the number of state entries written; do not rely on it for business-specific counts.
# IUnitOfWork

> **File:** `src/api/Gabriel.Core/Repositories/IUnitOfWork.cs`  
> **Kind:** interface

Coordinates persistence across repositories and defines a single commit boundary for a use-case. Implementations persist all in-flight changes as one logical transaction; application services should call SaveChangesAsync at the end of a unit-of-work to atomically apply modifications made through multiple repositories.

## Remarks
This abstraction represents the commit boundary for a business operation and decouples service logic from concrete persistence details (ORMs, database transactions, etc.). Use it when a use-case involves multiple repositories or aggregate roots and you want a single point to flush changes and control transactional behavior. Implementations typically delegate to an underlying data context and may open/commit a transaction as needed.

## Example
```csharp
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orders;

    public OrderService(IUnitOfWork unitOfWork, IOrderRepository orders)
    {
        _unitOfWork = unitOfWork;
        _orders = orders;
    }

    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        _orders.Add(order);
        // make other repository changes as needed...

        // Persist all changes as a single unit of work
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

## Notes
- The returned int typically represents the number of state entries written to the store; it can be 0 if nothing changed.
- CancellationToken is forwarded to the async persistence operation; callers should pass the request or operation token to allow cancellation.
- Transactional behavior (explicit transactions, retry policies, exception types) is implementation-specific — do not assume semantics beyond "commit the current unit of work."

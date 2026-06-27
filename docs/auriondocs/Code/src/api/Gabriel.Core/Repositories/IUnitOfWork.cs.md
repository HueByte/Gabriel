# IUnitOfWork

> **File:** `src/api/Gabriel.Core/Repositories/IUnitOfWork.cs`  
> **Kind:** interface

Coordinates persistence across repositories and defines a single commit point for a use‑case. Services call SaveChangesAsync once per logical operation to persist in‑flight changes as an atomic operation; the operation is asynchronous and accepts a CancellationToken to allow cooperative cancellation.

## Remarks
This interface establishes a unit-of-work abstraction that centralizes the transaction boundary for a use‑case and decouples service logic from any specific ORM or database transaction API. It is intended to be used alongside repository abstractions so multiple repositories can participate in the same commit. Concrete implementations are responsible for the actual transaction semantics and persistence behavior.

## Example
```csharp
public class MyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _users;

    public MyService(IUnitOfWork unitOfWork, IUserRepository users)
    {
        _unitOfWork = unitOfWork;
        _users = users;
    }

    public async Task CreateUserAsync(UserDto dto, CancellationToken ct)
    {
        _users.Add(new User(dto.Name));
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
```

## Notes
- The integer returned by SaveChangesAsync is implementation-defined; do not assume a specific meaning unless documented by the concrete implementation.
- The CancellationToken is provided so callers can cancel the asynchronous commit; how cancellation affects the outcome (rolled back, partial, none) depends on the implementation.
- UnitOfWork instances are typically scoped to a single logical operation or request and are generally not intended for concurrent use from multiple threads.
# UnitOfWork

> **File:** `src/api/Gabriel.Infrastructure/Persistence/UnitOfWork.cs`  
> **Kind:** class

A thin adapter that implements IUnitOfWork by delegating SaveChangesAsync to an injected AppDbContext. Use this when application code (services, handlers, or repositories) should depend on an abstraction for committing EF Core changes rather than on DbContext directly, enabling easier testing and separation of concerns.

## Remarks
This class exists primarily to hide the concrete DbContext behind an interface so higher-level code can request IUnitOfWork in its constructor and remain decoupled from EF Core. It performs no additional orchestration: it does not start or manage database transactions, track repositories, or wrap SaveChanges in retries — it simply forwards the call to AppDbContext.SaveChangesAsync.

## Example
```csharp
// Typical usage via dependency injection in a service
public class UserService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;

    public UserService(IUnitOfWork uow, IUserRepository users)
    {
        _uow = uow;
        _users = users;
    }

    public async Task CreateUserAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user);
        await _uow.SaveChangesAsync(ct);
    }
}
```

## Notes
- This implementation does not provide transactional guarantees beyond what AppDbContext offers; if you need an explicit transaction scope, begin one on the DbContext before calling SaveChangesAsync.
- AppDbContext (and therefore this UnitOfWork) is not thread-safe — do not reuse the same instance concurrently across threads.
- The provided CancellationToken is forwarded straight to DbContext.SaveChangesAsync.

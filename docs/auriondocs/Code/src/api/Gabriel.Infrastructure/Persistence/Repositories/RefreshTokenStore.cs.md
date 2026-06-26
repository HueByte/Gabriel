# RefreshTokenStore

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs`  
> **Kind:** class

A lightweight repository over an EF Core AppDbContext that provides basic operations for RefreshToken entities: lookup by token hash, adding a new token, and bulk-revoking all active tokens for a user. Use this when you want a simple data-access abstraction for refresh tokens that delegates to the application's DbContext rather than re-implementing EF Core queries.

## Remarks
This class encapsulates the persistence details for refresh tokens so higher-level authentication logic can remain database-agnostic. It uses `DbSet<T>` queries directly and, for the revoke operation, leverages ExecuteUpdateAsync to perform a single-server-side UPDATE without loading entities into the change tracker, which is more efficient for bulk revocation.

## Example
```csharp
// typical usage inside a service with DI-provided AppDbContext / IRefreshTokenStore
public class AuthService
{
    private readonly IRefreshTokenStore _tokens;
    private readonly AppDbContext _ctx; // if you need to commit changes

    public AuthService(IRefreshTokenStore tokens, AppDbContext ctx)
    {
        _tokens = tokens;
        _ctx = ctx;
    }

    public async Task IssueTokenAsync(RefreshToken token)
    {
        await _tokens.AddAsync(token);
        await _ctx.SaveChangesAsync(); // persist the added token
    }

    public Task<RefreshToken?> FindAsync(string tokenHash)
        => _tokens.FindByHashAsync(tokenHash);

    public Task RevokeAll(Guid userId)
        => _tokens.RevokeAllForUserAsync(userId);
}
```

## Notes
- AddAsync only stages the entity with the DbContext; call SaveChangesAsync on the context to persist it.
- RevokeAllForUserAsync uses ExecuteUpdateAsync (server-side UPDATE). That means any RefreshToken entities already tracked by the current DbContext will not have their in-memory values updated automatically — reload them if you rely on tracked state.
- Revocation sets RevokedAt = DateTimeOffset.UtcNow and only affects tokens with RevokedAt == null; the method is idempotent for already-revoked tokens.
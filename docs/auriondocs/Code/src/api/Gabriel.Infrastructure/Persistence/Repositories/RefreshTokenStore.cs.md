# RefreshTokenStore

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs`  
> **Kind:** class

```csharp
public class RefreshTokenStore : IRefreshTokenStore
```


RefreshTokenStore is a concrete implementation of IRefreshTokenStore that persists RefreshToken entities via AppDbContext. It provides three asynchronous operations: FindByHashAsync to locate a token by its hash, AddAsync to persist a new token, and RevokeAllForUserAsync to revoke all active tokens for a particular user. The revoke operation performs a bulk update to mark RevokedAt for matching tokens in a single database call, avoiding loading rows into the change tracker.

## Remarks
This class serves as a focused persistence adapter over EF Core, isolating the storage concerns of refresh tokens behind the IRefreshTokenStore contract. By using a bulk update via ExecuteUpdateAsync for revocation, it minimizes memory usage and round-trips while ensuring tokens are consistently marked as revoked. Callers can swap this store for another persistence strategy without changing domain code.

## Notes
- AddAsync queues the new token for insertion; the actual database insert occurs when the context is saved (SaveChanges/SaveChangesAsync).
- RevokeAllForUserAsync uses a single UPDATE statement via ExecuteUpdateAsync, avoiding per-token reads.
- It revokes only tokens where RevokedAt is null, making the operation idempotent for already revoked tokens.
- Cancellation tokens are threaded through all EF Core calls to support cooperative cancellation.
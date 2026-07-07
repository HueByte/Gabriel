# RefreshTokenStore

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs`  
> **Kind:** class

```csharp
public class RefreshTokenStore : IRefreshTokenStore
```


RefreshTokenStore is a concrete EF Core-backed implementation of IRefreshTokenStore that persists and manages refresh tokens via AppDbContext. It exposes methods to locate a token by its hash, add new tokens, and revoke all active tokens for a specific user in a single, efficient operation. Use this class when you need durable, database-backed token lifecycle management rather than ad-hoc in-memory handling.

## Remarks
This class centralizes token persistence behind IRefreshTokenStore, isolating domain logic from EF Core specifics. RevokeAllForUserAsync performs a bulk update using EF Core's ExecuteUpdateAsync, avoiding loading tokens into the change tracker and reducing memory pressure for revocation. The timestamp RevokedAt is set using DateTimeOffset.UtcNow to provide a consistent, timezone-agnostic record of when a token was revoked. Note that AddAsync marks an entity for insertion; the caller must call SaveChangesAsync to persist new tokens.

## Example
```csharp
// Retrieve a token by its hash, then revoke all tokens for that user
var token = await refreshTokenStore.FindByHashAsync(tokenHash, ct);
if (token != null)
{
    await refreshTokenStore.RevokeAllForUserAsync(token.UserId, ct);
}
```

## Notes
- AddAsync adds the token to the DbContext; persistence occurs when SaveChangesAsync is called by the caller.
- RevokeAllForUserAsync relies on EF Core 7+'s ExecuteUpdateAsync to perform a bulk update; ensure your project targets a compatible EF Core version.
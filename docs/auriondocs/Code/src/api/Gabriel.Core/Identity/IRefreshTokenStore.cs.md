# IRefreshTokenStore

> **File:** `src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs`  
> **Kind:** interface

Provides the persistence operations required to store and manage refresh tokens. Use this interface when implementing the storage layer for refresh tokens so higher-level services (for example JwtTokenService) can look up a token by its hashed value, add a new token, or bulk-revoke every active token for a user.

## Remarks
This interface represents the persistence boundary for RefreshToken entities. Implementations are expected to be used as part of the application's unit-of-work/transaction pattern so compound operations — for example rotating a token by marking the old one replaced and inserting a new one — can commit atomically. RevokeAllForUserAsync exists to support bulk revocation scenarios (user-initiated logout, account compromise recovery, theft-detection flows) and should be implemented as an efficient set-based update where possible.

## Example
```csharp
// Typical usage from a higher-level service:
var existing = await refreshTokenStore.FindByHashAsync(tokenHash, ct);
if (existing != null)
{
    // validate existing token, optionally rotate
    await refreshTokenStore.AddAsync(new RefreshToken(/* ... */), ct);
}

// Revoke every active refresh token for a user (logout-all-sessions / theft handling)
await refreshTokenStore.RevokeAllForUserAsync(userId, ct);
```

## Notes
- All methods accept a CancellationToken; callers should forward cancellation to avoid long-running storage operations.
- FindByHashAsync returns null when no token matches the provided hash.
- RevokeAllForUserAsync can affect many rows; implementations should perform efficient, set-based updates and ensure appropriate indexes to avoid performance issues.
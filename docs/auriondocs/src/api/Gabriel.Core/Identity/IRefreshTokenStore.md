# IRefreshTokenStore

> **File:** `src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs`  
> **Kind:** interface

A persistence abstraction for refresh tokens used by authentication components (for example JwtTokenService). Use this interface when you need to look up a stored refresh token by its hash, persist a newly issued refresh token, or bulk-revoke every active refresh token for a given user (for example during account recovery or theft-detection).

## Remarks
This interface defines the persistence boundary for refresh-token operations so higher-level services do not depend on storage specifics. The implementation is expected to participate in the application's unit-of-work pattern so multi-step rotations (marking an old token as replaced and inserting the replacement) can be committed atomically. RevokeAllForUserAsync centralizes the bulk-revocation step used by account-wide logout or compromise-handling flows.

## Example
```csharp
// Conceptual example: rotate a refresh token inside a unit-of-work so the mark-old + insert-new commit together
public async Task RotateRefreshTokenAsync(string oldTokenHash, RefreshToken newToken, IRefreshTokenStore store, IUnitOfWork uow, CancellationToken ct)
{
    var old = await store.FindByHashAsync(oldTokenHash, ct);
    if (old != null)
    {
        old.ReplacedBy = newToken.Id; // or old.IsActive = false; depends on model
        // persist changes via the unit-of-work's context
    }

    await store.AddAsync(newToken, ct);
    await uow.CommitAsync(ct);
}
```

## Notes
- FindByHashAsync returns null when no matching hashed token exists; callers must pass the token's hash (not the raw token) if the system stores only hashes.
- All methods are asynchronous and accept a CancellationToken; callers should propagate cancellation where appropriate.
- RevokeAllForUserAsync is intended to revoke every active token for the specified user and can be an expensive operation; prefer revoking a single token when possible and run bulk revocation within a unit-of-work if it must be combined with other changes.
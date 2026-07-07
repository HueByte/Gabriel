# IRefreshTokenStore

> **File:** `src/api/Gabriel.Core/Identity/IRefreshTokenStore.cs`  
> **Kind:** interface

```csharp
public interface IRefreshTokenStore
```


IRefreshTokenStore is the persistence boundary for refresh tokens. It provides asynchronous operations to locate a token by its hash, persist a new token, and bulk-revoke tokens for a user, with the rotation of tokens implemented atomically when saved through a unit of work.

## Remarks
This abstraction decouples the domain logic (for example, the JwtTokenService) from the details of how tokens are stored, enabling easier testing and the ability to swap storage backends without changing business logic. The FindByHashAsync/AddAsync trio supports the typical refresh-token rotation workflow, where an old token is replaced with a newly issued one within a single transactional boundary, and RevokeAllForUserAsync offers a bulk operation used in security-sensitive flows (such as theft-detection) to invalidate all tokens associated with a user.

## Example
```csharp
// Common rotation path: the store handles marking the old token as replaced and inserting the new one atomically
var newToken = new RefreshToken(userId, newHash, expiresAt);
await tokenStore.AddAsync(newToken, ct);

// Theft-detection path: invalidate all tokens for a user in one operation
await tokenStore.RevokeAllForUserAsync(userId, ct);
```

## Notes
- FindByHashAsync returns null if the token hash is not found; callers should handle the possibility of a null result.
- RevokeAllForUserAsync is a bulk operation; use it with care, as it invalidates all tokens for the specified user across devices.
- All operations are designed to participate in a unit-of-work; token rotation relies on the store to commit the old-and-new state atomically.
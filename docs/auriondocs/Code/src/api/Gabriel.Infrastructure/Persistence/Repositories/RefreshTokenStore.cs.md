# RefreshTokenStore

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs`  
> **Kind:** class

```csharp
public class RefreshTokenStore : IRefreshTokenStore
```


RefreshTokenStore is a data-access class that implements IRefreshTokenStore and encapsulates EF Core operations for RefreshToken entities using AppDbContext. It exposes the minimal set of operations needed by the domain: locate a token by its hash, add a new token, and revoke all non-revoked tokens for a specific user. Use this store when you need to interact with refresh tokens from the persistence layer (instead of talking to DbContext directly), providing a single place for token-related queries and updates.

## Remarks

By centralizing token persistence behind this repository, you gain testability and a clearer boundary between domain logic and data access. The RevokeAllForUserAsync method uses EF Core's bulk update (ExecuteUpdateAsync) to mark all active tokens for a user as revoked in a single database operation, avoiding loading tokens into memory. The timestamp uses UTC to avoid mixed timezones.

## Example

```csharp
// Revoke all refresh tokens for a user after a logout or credential change
await refreshTokenStore.RevokeAllForUserAsync(userId, cancellationToken);
```

```csharp
// Example: adding a new refresh token (persistence requires SaveChanges on the DbContext)
var token = new RefreshToken { UserId = userId, TokenHash = hash };
await refreshTokenStore.AddAsync(token, cancellationToken);
await context.SaveChangesAsync(cancellationToken);
```

## Notes

- AddAsync does not persist until SaveChangesAsync is called on the DbContext.
- RevokeAllForUserAsync uses a bulk update; no per-token change-tracker events are raised.
- This method affects only tokens with RevokedAt == null for the specified user.
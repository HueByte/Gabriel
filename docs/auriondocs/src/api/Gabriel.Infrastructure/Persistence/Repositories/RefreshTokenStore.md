# RefreshTokenStore

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/RefreshTokenStore.cs`  
> **Kind:** class

Stores and manages RefreshToken entities using an EF Core AppDbContext. Use this repository when you need a simple persistence abstraction for finding a token by its hash, adding a new token to the context, or revoking all active tokens for a given user in a single bulk database update.

## Remarks
This class is a thin repository over AppDbContext focused on refresh-token operations. It deliberately uses a bulk update (ExecuteUpdateAsync) to revoke tokens without loading rows into the change tracker, which is more efficient for mass revocation. Adding a token uses AddAsync so the entity is attached to the context but not persisted until SaveChanges/SaveChangesAsync is called by the caller.

## Example
```csharp
var store = new RefreshTokenStore(dbContext);

// add a token (remember to save changes)
await store.AddAsync(new RefreshToken { UserId = userId, TokenHash = hash });
await dbContext.SaveChangesAsync();

// look up a token by its hash
var token = await store.FindByHashAsync(hash);

// revoke all active tokens for a user (commits immediately)
await store.RevokeAllForUserAsync(userId);
```

## Notes
- AddAsync only attaches the entity to the DbContext; call SaveChanges/SaveChangesAsync to persist it.
- RevokeAllForUserAsync sets RevokedAt to DateTimeOffset.UtcNow in the database with a single UPDATE; it does not load entities into the change tracker and therefore in-memory tracked entities will not be automatically updated.
- CancellationToken parameters are honored by the underlying EF Core calls.
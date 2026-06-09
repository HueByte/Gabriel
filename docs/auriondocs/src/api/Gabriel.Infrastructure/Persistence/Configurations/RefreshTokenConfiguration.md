# RefreshTokenConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`  
> **Kind:** class

Configures the EF Core mapping for the RefreshToken entity: table name, primary key, required columns, column constraints (TokenHash length) and indexes (unique TokenHash for lookup, non-unique UserId for bulk operations). Use this configuration from your DbContext to centralize and reuse the entity mapping instead of defining the mappings inline.

## Remarks
This class encapsulates the persistence rules for refresh tokens so the model configuration is kept declarative and testable. It enforces a unique lookup path by TokenHash (used on every token refresh) and a separate index on UserId for bulk-revoke or audit queries. Properties RevokedAt and ReplacedByTokenId are left optional to represent tokens that are still active or not replaced.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
}
```

## Notes
- TokenHash has a MaxLength(128): ensure the hashing/encoding strategy you use produces a value that fits this limit (or adjust the configuration).
- The unique index on TokenHash prevents storing two tokens with the same hash; hash collisions or duplicate inserts will fail with a constraint error.
- RevokedAt and ReplacedByTokenId are nullable in the model (not marked as required here), so absence indicates an active, unreplaced token.
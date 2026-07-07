# RefreshTokenConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`  
> **Kind:** class

```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
```


Configures EF Core's Fluent API for the RefreshToken entity and maps it to the RefreshTokens table. It declares the primary key and a set of required and optional properties, and defines indexes to support lookups by token hash and by user for revoke/monitoring operations.

## Remarks
This abstraction centralizes persistence concerns for the RefreshToken aggregate, decoupling schema decisions from the domain model. By combining table mapping, key constraints, and targeted indexes, it optimizes common refresh-token workflows (validation by hash and user-scoped revocation scans) while keeping the domain entity simple.

## Example
```csharp
// Typical usage in your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
}
```

## Notes
- Ensure hashing of the refresh token yields a string within the TokenHash max length (128).
- RevokedAt and ReplacedByTokenId are nullable to support pending or historical states; account for nulls in queries.
- The unique index on TokenHash enforces that each refresh token hash is stored exactly once, while the UserId index supports efficient user-scoped revocation scans.
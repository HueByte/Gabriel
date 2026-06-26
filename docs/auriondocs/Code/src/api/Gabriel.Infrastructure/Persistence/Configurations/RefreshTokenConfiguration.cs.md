# RefreshTokenConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`  
> **Kind:** class

Configures the Entity Framework Core mapping for the RefreshToken entity: sets the table name, primary key, required properties, column constraints (TokenHash length), and two indexes (unique TokenHash for fast lookup and a non-unique UserId index for bulk operations). Use this configuration when registering entity mappings in your DbContext (e.g., inside OnModelCreating via modelBuilder.ApplyConfiguration).

## Remarks
This class centralizes persistence concerns for refresh tokens so the entity definition stays focused on domain behavior. The configuration intentionally keeps the TokenHash index lean and unique to make single-token lookups (token validation) fast and selective, while a separate non-unique index on UserId supports user-scoped operations such as bulk revocation or theft-detection scans.

## Example
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
}
```

## Notes
- TokenHash is constrained to 128 characters; ensure your hashing/encoding produces values that fit this length.
- The unique TokenHash index implies two different refresh tokens cannot share the same hash value; inserting duplicates will fail.
- RevokedAt and ReplacedByTokenId are configured without IsRequired(), so they are nullable in the database and indicate optional revocation/rotation state.
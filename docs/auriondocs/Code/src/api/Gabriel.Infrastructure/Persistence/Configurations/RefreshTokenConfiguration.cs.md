# RefreshTokenConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`  
> **Kind:** class

```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
```


Configures how the RefreshToken entity is persisted by EF Core: maps it to the RefreshTokens table, enforces a primary key and required fields, and applies indices for lookup and user-scoped queries. This configuration is consulted when the model is built, ensuring data integrity and efficient access for token validation, revocation, and replacement workflows.

## Remarks
By centralizing schema details in one place, this configuration ensures the RefreshToken table enforces a stable contract: required fields, a fixed token hash length, and predictable keys. The unique index on TokenHash guards against duplicate tokens and makes token validation efficient, while the UserId index supports per-user lookups for revocation and hygiene tasks.

## Notes
- The unique index on TokenHash means two tokens with the same hash cannot be stored; ensure token hashing and lifetime logic minimize collisions.
- CreatedAt and ExpiresAt are required; ensure tokens are assigned these values at creation to avoid constraint violations.

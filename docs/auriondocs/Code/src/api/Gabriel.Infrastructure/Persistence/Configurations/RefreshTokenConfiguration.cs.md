# RefreshTokenConfiguration

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`  
> **Kind:** class

```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
```


Configures how the RefreshToken entity is persisted by EF Core. It specifies the table name, primary key, required fields, optional fields, and two indices that optimize refresh-token lookups and per-user scans. Use this configuration when you want to keep the database schema in sync with the RefreshToken domain entity, ensuring that tokens are stored with creation and expiration timestamps, and that revocation or replacement can be represented without breaking existing data.

## Remarks
This abstraction centralizes persistence rules for refresh tokens, reducing drift between the domain model and the database. By anchoring the table name, constraints, and indices in one place, migrations become predictable and the querying paths (by token hash and by user) stay performant across contexts.

## Notes
- TokenHash is required and has a maximum length of 128 characters; ensure your hashing output conforms to this limit to avoid runtime failures.
- A unique index on TokenHash enables fast, collision-resistant lookups and enforces token uniqueness at the database level.
- CreatedAt and ExpiresAt are required, while RevokedAt and ReplacedByTokenId are optional, supporting revocation and token replacement scenarios without forcing data loss.
- The table is named RefreshTokens to align with conventional pluralized naming; if a different naming is required, migrations or a different configuration approach should be used.
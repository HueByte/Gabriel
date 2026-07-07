# IdentitySeeder

> **File:** `src/api/Gabriel.API/Identity/IdentitySeeder.cs`  
> **Kind:** class

```csharp
public static class IdentitySeeder
```


IdentitySeeder is an idempotent startup helper that seeds a single Identity user after EF migrations when Auth.Seed is enabled and properly configured; it creates the user with the configured UserName, Email, and Password, hashing the password via Identity upon creation. If seeding is disabled, misconfigured, or the user already exists, the operation is a no-op with informative logging.

## Remarks
IdentitySeeder encapsulates the bootstrapping of an Identity user and keeps seed data out of migrations and startup flow. It relies on DI services like `UserManager<ApplicationUser>` and ILogger to perform membership operations and to report status, ensuring the seed is applied exactly once in a predictable manner. This pattern is particularly useful for development and test environments where a known administrator or test account is required.

## Example
```csharp
// Typical usage during application startup
using var scope = app.Services.CreateScope();
await IdentitySeeder.SeedAsync(scope.ServiceProvider);
```

## Notes
- If Auth:Seed:Enabled is false, seeding is skipped (no-op).
- If UserName or Password is blank, seeding is skipped with a warning.
- If creation fails due to Identity errors (e.g., password policy or email/username conflicts), the detailed error messages are logged.
# IdentitySeeder

> **File:** `src/api/Gabriel.API/Identity/IdentitySeeder.cs`  
> **Kind:** class

```csharp
public static class IdentitySeeder
```


IdentitySeeder is an idempotent startup seeder that runs after EF migrations to ensure a configured Identity user exists. It reads AuthOptions.Seed to decide whether to seed; if seeding is disabled, it becomes a no-op. When enabled, it validates that UserName and Password are configured; if either is blank, it logs a warning and returns without creating a user. If a user with the configured UserName already exists, it performs no action. Otherwise, it creates a new ApplicationUser with UserName and Email, marks EmailConfirmed as true, and creates the user using UserManager with the configured plaintext password; Identity hashes the password automatically via the registered PasswordHasher. The operation logs outcomes and surfaces errors if creation fails.

## Remarks
IdentitySeeder centralizes the initial identity state for the application in a predictable startup step that runs after the database schema is ready. It encapsulates the seeding policy (enabled/disabled, required fields, and idempotency) in a single place, reducing scattered initialization logic across the codebase. It relies on DI to obtain the Auth options, a logger, and the UserManager, and it communicates outcomes via the Gabriel.IdentitySeeder logger category to aid traceability.

## Notes
- Be aware that enabling seeding with a missing UserName or Password will log a warning and skip seeding.
- The seed uses the configured plaintext password; Identity hashes it upon creation.
- Seeding runs after EF migrations to guarantee the Users table exists; if the configured user already exists, seeding is skipped.
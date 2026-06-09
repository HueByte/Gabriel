# IdentitySeeder

> **File:** `src/api/Gabriel.API/Identity/IdentitySeeder.cs`  
> **Kind:** class

Creates an initial Identity user from configuration when seeding is explicitly enabled. Call this from application startup (after EF migrations have run) to idempotently ensure a configured user exists; the method is safe to run repeatedly because it skips creation if the user already exists.

## Remarks
This helper centralizes bootstrap logic for a single configured seed account (typically an initial admin). It validates that seeding is enabled and that both UserName and Password are provided before attempting creation, avoiding accidental creation of a partially-initialized account. User creation is performed through ASP.NET Core Identity's UserManager (so the registered password hasher and password policy apply), and the method logs each outcome.

## Example
```csharp
// In Program.cs or during app startup, after applying EF migrations:
await IdentitySeeder.SeedAsync(app.Services, cancellationToken);
```

## Notes
- Ensure Auth:Seed:Enabled is set to true and both UserName and Password are configured (env vars: AUTH__SEED__USERNAME / AUTH__SEED__PASSWORD) or the seeder will skip and log a warning.
- If the configured password violates Identity password policy or the email conflicts with an existing user, CreateAsync will fail; the method logs errors with Identity's descriptions.
- Run this after migrations so the Users table exists; the seeder assumes the Identity stores are ready.
- The method is idempotent: it checks for an existing user by UserName and skips creation if found.

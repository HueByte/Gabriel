# IdentitySeeder

> **File:** `src/api/Gabriel.API/Identity/IdentitySeeder.cs`  
> **Kind:** class

Creates a single initial user from configuration if one does not already exist. Use this at application startup (after EF migrations) when you want an idempotent, configuration-driven way to ensure an initial account is present (for local dev, testing, or first-time deployments) without embedding plaintext password logic into your startup code.

## Remarks
This is a minimal, safety-minded seeder that relies on the application's configured ASP.NET Core Identity services. It reads AuthOptions.Seed from `IOptions<AuthOptions>`, short-circuits when seeding is disabled or misconfigured, and uses `UserManager<ApplicationUser>` to create the account so the registered PasswordHasher and Identity password policies are applied. Logging is used to surface why seeding was skipped or failed.

## Example
```csharp
// In Program.cs or wherever you run startup tasks, after applying migrations:
using var scope = app.Services.CreateScope();
await IdentitySeeder.SeedAsync(scope.ServiceProvider, cancellationToken);
```

## Notes
- The caller must ensure database migrations have run before invoking this seeder; it assumes the Users table exists.
- If the configured password violates Identity password policy or the email/username uniqueness rules, creation will fail and be logged — the seeder does not retry or override policies.
- The method accepts a CancellationToken but does not pass it to UserManager calls; cancellation is currently not observed by the internal Identity operations.
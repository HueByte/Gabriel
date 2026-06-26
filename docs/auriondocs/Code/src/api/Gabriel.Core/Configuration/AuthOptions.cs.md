# AuthOptions.cs

> **Source:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`

## Contents

- [AuthOptions](#authoptions)
- [SeedUserOptions](#seeduseroptions)

---

## AuthOptions

> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

Holds application-level authentication configuration that controls two surface-level behaviors: whether new user registration is allowed and the parameters for an optional bootstrap (seed) user. Reach for this class when you need to toggle the public registration endpoint for single-tenant or private deployments, or to configure a seeded account created at startup.

## Remarks
This class centralizes small auth-related toggles that are not part of JWT/token issuance (for example, the token signing keys or token lifetimes). The SectionName property exposes the configuration section key ("Auth") so the type can be bound from configuration during startup. The Seed property represents an idempotent bootstrap user; the application will skip seeding if a user with the configured UserName already exists.

## Example
```csharp
// Bind directly from IConfiguration
var authOptions = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>();

// Or register with the DI options system
services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
```

## Notes
- RegistrationEnabled defaults to true; setting it to false disables POST /api/auth/register (returns 403) but does not affect login/refresh/logout endpoints.
- Seed is initialized to a new SeedUserOptions instance by default and is applied idempotently at startup (skipped when a user with the configured UserName exists).
- SectionName is the literal configuration key ("Auth") used for binding.

---

## SeedUserOptions

> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

Holds configuration for an opt-in, deploy-time "seed" Identity user used to create a single account from configuration. Reach for this type when you want to provision an initial administrative or service account from app configuration (or secrets) rather than creating it interactively; the class encodes the minimal fields the seeding logic needs and a small convenience API for validation.

## Remarks
This class is a small POCO intended to be populated from configuration (for example appsettings or environment-backed configuration providers) and consumed by a startup/seed routine. The Enabled flag prevents accidental account creation by default; ResolvedEmail mirrors the registration behavior by falling back to UserName when Email is blank. Password is stored as plaintext only in configuration and must be handed to Identity's PasswordHasher when creating the user — the type itself never hashes or logs the password.

## Example
```csharp
// Typical usage in startup/seed code
var seed = configuration.GetSection("SeedUser").Get<SeedUserOptions>();
if (seed?.IsConfigured == true && seed.Enabled)
{
    var email = seed.ResolvedEmail;
    // Create user via your UserManager<ApplicationUser>
    // var user = new ApplicationUser { UserName = seed.UserName, Email = email };
    // await userManager.CreateAsync(user, seed.Password);
}
```

## Notes
- Enabled defaults to false to avoid silently creating accounts in misconfigured deployments.
- ResolvedEmail returns Email when present, otherwise UserName — ensure this matches your Identity validation rules (RequireUniqueEmail). 
- Password is plaintext only until handed to Identity; do not log it and prefer secure configuration providers (user secrets, vaults) in production.
- IsConfigured requires Enabled && non-empty UserName && non-empty Password; Email may be blank but must be a valid email if your Identity configuration enforces unique/valid emails.

---
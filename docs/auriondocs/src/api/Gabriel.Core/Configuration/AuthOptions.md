# AuthOptions.cs

> **Source:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`

## Contents

- [AuthOptions](#authoptions)
- [SeedUserOptions](#seeduseroptions)

---

## AuthOptions

> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

Holds runtime configuration for authentication-related surface flags that are not part of JWT issuance itself — primarily a registration enable/disable toggle and options for a bootstrap (seed) user. Reach for this type when you need to read or bind the application's "Auth" configuration section (for example to disable self-registration in single-tenant/private deployments or to configure a startup seed user).

## Remarks
Centralizes non-token auth knobs so callers (startup code, admin UIs, tests) can treat registration and bootstrapping consistently. The SectionName property identifies the configuration section ("Auth") that this type binds to. The class deliberately separates the registration kill-switch from login/refresh/logout behavior: disabling registration prevents new accounts from being created via POST /api/auth/register but does not block existing accounts from authenticating or refreshing tokens.

## Example
```csharp
// appsettings.json (conceptual)
// "Auth": { "RegistrationEnabled": false, "Seed": { "UserName": "admin", "Password": "..." } }

// Binding in startup
var auth = configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>();
if (!auth.RegistrationEnabled) {
    // registration endpoint should return 403 (handled by auth layer elsewhere)
}

// The seeding logic (run at startup) should read auth.Seed and create the user
// only if a user with the configured UserName does not already exist.
```

## Notes
- RegistrationEnabled only affects the registration endpoint; login/refresh/logout remain functional.
- The seed user is created at startup by separate seeding logic and is idempotent — it is skipped if a user with the configured UserName already exists.
- Do not commit real seeded credentials to source control; treat any configured seed password as sensitive configuration.


---

## SeedUserOptions

> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

Represents configuration for an optional, single seeded Identity user that can be created at application startup. Use this when you want to provision an initial account (for example an admin or test user) via configuration; the feature is opt-in to avoid accidental account creation in misconfigured deployments.

## Remarks
This type exists to keep seeding explicit and safe: seeding must be deliberately enabled and provide at least a username and password. It mirrors the application registration conventions where UserName is the identity key (and appears in JWTs) while Email is what the login lookup uses; they may differ so a deployment can use a friendly UserName (e.g. "admin") while still supplying a real mailbox for Email.

## Example
```csharp
// appsettings.json
"SeedUser": {
  "Enabled": true,
  "UserName": "admin",
  "Email": "admin@example.com",
  "Password": "ChangeMe123!"
}

// Startup or seeding code
var seedOptions = configuration.GetSection("SeedUser").Get<SeedUserOptions>();
if (seedOptions.IsConfigured)
{
    // Use seedOptions.UserName, seedOptions.ResolvedEmail, seedOptions.Password
    // Create the Identity user; Password will be hashed by the configured PasswordHasher.
}
```

## Notes
- Enabled defaults to false to prevent silent creation of an account from example or placeholder config.
- ResolvedEmail returns Email when provided, otherwise falls back to UserName (this mirrors registration behavior).
- The Password property holds plaintext only while creating the Identity user; the system's PasswordHasher should hash it before storage. Also, if the deployment uses RequireUniqueEmail, Email must be a valid/parsable email when Identity performs validation.

---
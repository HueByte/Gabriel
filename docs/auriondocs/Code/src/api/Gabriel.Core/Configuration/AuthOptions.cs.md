# AuthOptions.cs

> **Source:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`

## Contents

- [AuthOptions](#authoptions)
- [SeedUserOptions](#seeduseroptions)

---

## AuthOptions
> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

```csharp
public class AuthOptions : IConfigSection<AuthOptions>
```


AuthOptions groups authentication-related configuration knobs that are not part of the JWT issuance flow, including a startup toggle to enable or disable user registration (RegistrationEnabled) and a seed bootstrap user (Seed) created at startup; the seed is idempotent and skipped if a user with the configured UserName already exists. It implements [`IConfigSection<AuthOptions>`](IConfigSection.cs.md) and exposes the SectionName Auth so the framework can bind configuration sections (for example from appsettings.json) into this strongly typed object.

## Remarks
AuthOptions decouples operational authentication settings from the validation and issuance logic, allowing operators to toggle registration and seed behavior without touching runtime auth code. Placing these controls in a dedicated config section improves transparency and supports consistent initialization across deployments.

## Notes
- The RegistrationEnabled switch acts as a kill-switch for the registration endpoint: when false, POST /api/auth/register returns 403 while login/refresh/logout remain available.
- Seed bootstrap is idempotent and skipped if the seed user already exists.

---

## SeedUserOptions
> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

```csharp
public class SeedUserOptions
```


SeedUserOptions is a lightweight configuration object used to seed a user into the identity system during deployment. It exposes an opt-in Enabled flag to prevent silent account creation, and when Enabled is true and both UserName and Password are provided, the seed process can create an initial user whose identity is defined by UserName and, optionally, Email for login. ResolvedEmail returns the effective login email by preferring Email and falling back to UserName. IsConfigured reports readiness for seeding: Enabled is true and UserName and Password are non-empty. The Password field is plaintext on input; it is hashed by the configured PasswordHasher during seed, and never logged.

## Remarks
SeedUserOptions centralizes seed-time identity data and decouples seed logic from the domain user model, enabling safe defaults and explicit opt-in. It mirrors registration semantics by aligning the login target (ResolvedEmail/UserName) with how JWTs identify users, while enabling deployments to customize which field acts as the login email. This abstraction exists to let deployments opt into seeding a known administrator without leaking plaintext credentials or altering runtime user configuration.

## Notes
- Password is provided in plaintext and hashed via PasswordHasher; never log the plaintext.
- If Email is blank, ResolvedEmail uses UserName; ensure a meaningful UserName when Email is not supplied.
- Enabled is the gate for seeding; check IsConfigured to confirm readiness before performing seed operations.

---
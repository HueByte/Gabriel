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


AuthOptions is a configuration surface for authentication-related knobs that are not part of JWT issuance. It centralizes two controls: RegistrationEnabled, a kill-switch for the public /api/auth/register endpoint, and Seed, a bootstrap user created at startup. This class is intended for deployment scenarios (e.g., single-tenant or private deployments) where you want to limit new user creation to a seeded account and ensure a known initial user exists.

## Remarks
AuthOptions implements [`IConfigSection<AuthOptions>`](IConfigSection.cs.md), binding to the "Auth" configuration section to influence runtime behavior. The RegistrationEnabled flag lets operators disable public user registration while preserving login, refresh, and logout flows, enabling controlled access. The Seed option provides a deterministic startup bootstrap by creating a seed user if one does not already exist, simplifying initial setup and recovery across environments.

## Notes
- If RegistrationEnabled is false, POST /api/auth/register returns 403; login, token refresh, and logout continue to work with existing accounts.
- Seed is idempotent: at startup it will skip creating the seed user if a user with the configured username already exists, preventing duplicates on redeploys.


---

## SeedUserOptions
> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

```csharp
public class SeedUserOptions
```


SeedUserOptions is a lightweight data container used to drive the optional seeding of a user account in the authentication configuration. It exposes an opt-in flag (Enabled) so deployments cannot silently mint placeholder accounts, plus basic identity fields (UserName, Email) and a plaintext Password that will be hashed by the configured PasswordHasher during seed ingestion. ResolvedEmail derives the effective email-like key to use for lookups by preferring Email when provided and falling back to UserName when Email is blank, mirroring the behavior of the registration path. IsConfigured exposes a quick readiness check for seeding: Enabled must be true and both UserName and Password must be non-empty. This class is consumed by the seed path to determine whether to create a seed user and what credentials to seed, without entangling seed data with runtime identity logic.


---
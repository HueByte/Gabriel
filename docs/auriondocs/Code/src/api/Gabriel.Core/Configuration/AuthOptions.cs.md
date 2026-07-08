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


AuthOptions is a compact configuration surface for authentication-related behavior that sits alongside token issuance. Implementing [`IConfigSection<AuthOptions>`](IConfigSection.cs.md) enables binding from the application's configuration system under the Auth section, so operators can override behavior without code changes. It exposes two knobs: RegistrationEnabled, a kill-switch for the public registration endpoint, and Seed, a bootstrap user created at startup. Seed is idempotent: it will be skipped if a user with the configured UserName already exists. In short, this type consolidates deployment-time controls over who can register and which initial account exists, separate from how tokens are issued.

---

## SeedUserOptions
> **File:** `src/api/Gabriel.Core/Configuration/AuthOptions.cs`  
> **Kind:** class

```csharp
public class SeedUserOptions
```


SeedUserOptions is a configuration object that controls the optional seeding of a default Identity user for deployments. It is opt-in via the Enabled flag to prevent silent account creation in misconfigured environments; when Enabled is true, seeding uses UserName, Email, and Password to construct the seed user.

## Remarks
SeedUserOptions centralizes seed behavior so deployments can opt into a repeatable, auditable seed path without coupling to runtime user creation logic. It also clarifies how UserName, Email, and ResolvedEmail interact and why Password is accepted in plaintext only at input time (to be hashed by PasswordHasher).

## Example
```csharp
var options = new SeedUserOptions
{
    Enabled = true,
    UserName = "admin",
    Email = "admin@example.com",
    Password = "P@ssw0rd!"
};
```

## Notes
- Default Enabled is false to avoid silent minting of accounts.
- ResolvedEmail uses Email when provided; if Email is blank, it falls back to UserName.
- Password is provided in plaintext to the seeding path and is hashed by the configured PasswordHasher; never log the plaintext password.

---
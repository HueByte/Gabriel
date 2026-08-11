# JwtOptions

> **File:** `src/api/Gabriel.Core/Configuration/JwtOptions.cs`  
> **Kind:** class

```csharp
public class JwtOptions : IConfigSection<JwtOptions>
```


JwtOptions is a configuration model for JWT-related settings. It groups issuer, audience, signing key, and token lifetimes so token generation and inbound validation can be driven from a single typed source; use IsConfigured to guard against accidentally using an unset or too-short signing key.

## Remarks
This abstraction encapsulates the JWT configuration so the signing/validation flow can rely on a typed object rather than scattered literals. The SectionName drives binding to the "Jwt" configuration section; the defaults for AccessTokenMinutes (15) and RefreshTokenDays (30) reflect a conservative, short-lived token strategy. IsConfigured performs a lightweight sanity check that a sufficiently long SigningKey has been provided, but it does not validate cryptographic correctness.

## Example
```csharp
var options = new JwtOptions
{
    SigningKey = "abcdefghijklmnopqrstuvwxyz0123456789abcdef" // 32+ chars
};

if (options.IsConfigured)
{
    // proceed with token generation/validation
}
```

## Notes
- IsConfigured is a simple flag based solely on SigningKey length; it does not validate actual cryptographic material.
- Store SigningKey securely (e.g., user secrets or environment variables) and rotate it as part of your security hygiene.
# JwtOptions

> **File:** `src/api/Gabriel.Core/Configuration/JwtOptions.cs`  
> **Kind:** class

```csharp
public class JwtOptions : IConfigSection<JwtOptions>
```


JwtOptions is a configuration container for JWT-based authentication. It groups issuer, audience, signing key, and token lifetimes used when issuing and validating HS256-signed tokens. Implemented as [`IConfigSection<JwtOptions>`](IConfigSection.cs.md), it is designed to be bound from a configuration source (the Jwt section) and then consumed by your JWT token generator/validator. The IsConfigured property guards against using the signing key unless it is present and long enough (at least 32 characters).

## Remarks
JwtOptions centralizes JWT configuration in one place, reducing scattered configuration and magic values. It leverages the IsConfigured flag to provide a quick readiness check before initializing token generation/validation. By modeling the options as a configuration section, it aligns with your app’s configuration binding strategies, making rotation or updates to lifetimes straightforward.

## Example
```csharp
var options = new JwtOptions
{
    Issuer = "gabriel",
    Audience = "gabriel",
    SigningKey = "0123456789abcdef0123456789abcdef", // 32 chars
    AccessTokenMinutes = 15,
    RefreshTokenDays = 30
};

if (options.IsConfigured)
{
    // Use options with your JWT signing/validation logic
}
```

## Notes
- SigningKey must be at least 32 characters; IsConfigured will be false otherwise.
- Store SigningKey securely (environment variables, user secrets) and avoid hard-coding in source.

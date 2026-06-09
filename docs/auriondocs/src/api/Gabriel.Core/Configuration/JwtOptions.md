# JwtOptions

> **File:** `src/api/Gabriel.Core/Configuration/JwtOptions.cs`  
> **Kind:** class

Represents the application's JWT configuration and defaults used for token issuance and validation. Bind this to the "Jwt" configuration section (or register it with DI) when setting up authentication, token services and refresh-token handling.

## Remarks
Centralizes values that must match between token producers and consumers: Issuer, Audience and the HS256 SigningKey, plus token lifetimes. Defaults favor short-lived access tokens (15 minutes) and longer, revokable refresh tokens (30 days) with rotation. The IsConfigured property is a convenience guard that ensures a sufficiently long SigningKey is present before enabling JWT-based auth.

## Example
```csharp
// appsettings.json
{
  "Jwt": {
    "Issuer": "gabriel",
    "Audience": "gabriel",
    "SigningKey": "your-32+ character secret here...",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30
  }
}

// Program.cs / Startup.cs - bind and validate
var jwtOptions = new JwtOptions();
configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
if (!jwtOptions.IsConfigured)
{
    throw new InvalidOperationException("JWT signing key is not configured or is too short.");
}
services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
// Register authentication/token services that consume JwtOptions...
```

## Notes
- SigningKey must be at least 32 characters (256 bits) for HS256; validation will fail otherwise.
- Keep the SigningKey secret (use user-secrets, environment/Infisical: JWT__SIGNINGKEY, or a secret store); do not check it into source control.
- Changing or rotating the SigningKey will invalidate existing tokens; plan rotation and refresh flows accordingly.
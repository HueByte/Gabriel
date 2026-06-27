# JwtOptions

> **File:** `src/api/Gabriel.Core/Configuration/JwtOptions.cs`  
> **Kind:** class

Holds configuration for JWT issuance and validation used by the authentication/token code paths. Use this type when binding configuration (the section name is Jwt) or when passing a single typed settings object into components that create or validate JWTs (token issuer, authentication middleware, refresh token logic).

## Remarks
Centralizes the values that must match between token generation and validation: issuer, audience, the symmetric signing key, and token lifetimes. It provides a small convenience property (IsConfigured) to detect whether a minimally valid signing key has been supplied. The class is intended to be bound from configuration (appsettings, environment, or secrets) so callers can read one typed object rather than scattering magic strings and numbers across the codebase.

## Example
```csharp
// appsettings.json
// {
//   "Jwt": {
//     "Issuer": "gabriel",
//     "Audience": "gabriel",
//     "SigningKey": "<a-secure-32+ character secret>",
//     "AccessTokenMinutes": 15,
//     "RefreshTokenDays": 30
//   }
// }

// Program.cs / Startup.cs
var jwtOptions = new JwtOptions();
configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
if (!jwtOptions.IsConfigured) {
    throw new InvalidOperationException("JWT signing key is not configured or is too short.");
}

// Use jwtOptions when configuring token generation or the authentication middleware
// (passing Issuer, Audience, SigningKey-derived SymmetricSecurityKey, lifetimes, etc.).
```

## Notes
- The SigningKey must be at least 32 characters (256 bits) for HS256; IsConfigured only checks length and whitespace, not cryptographic strength.
- Never commit the SigningKey to source control; supply it via a secret manager/environment variables (e.g., user-secrets, Infisical) as suggested in the source comments.
- Access tokens are intentionally short-lived; refresh tokens are longer-lived but expected to be revocable and rotated by server-side logic (see related refresh token store/rotation code).
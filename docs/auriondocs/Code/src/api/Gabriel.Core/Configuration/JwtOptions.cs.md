# JwtOptions

> **File:** `src/api/Gabriel.Core/Configuration/JwtOptions.cs`  
> **Kind:** class

```csharp
public class JwtOptions : IConfigSection<JwtOptions>
```


JwtOptions is the configuration section that groups all JWT-related settings used to issue and validate tokens; it centralizes issuer, audience, signing key, and token lifetimes. The IsConfigured property signals whether a valid 32+ character SigningKey has been provided, guiding startup logic to enable or skip JWT-based authentication.

## Remarks
JwtOptions centralizes JWT policy in a single, strongly-typed object, simplifying how the rest of the system reads token settings. By exposing a simple IsConfigured flag, it helps startup logic decide whether JWT-based authentication should be enabled. The defaults provide baseline values for issuer, audience, and token lifetimes, while the signing key constraint ensures a minimum security level for HS256.

## Notes
- SigningKey must be loaded from a secure secret store (e.g., Infisical JWT__SIGNINGKEY) or user-secrets (Jwt:SigningKey); otherwise IsConfigured will be false.
- Avoid exposing the SigningKey in logs or error messages; rely on IsConfigured to indicate readiness instead of printing the key.
- If you rotate the signing key, update the configuration and re-check IsConfigured to ensure the change is picked up at startup.
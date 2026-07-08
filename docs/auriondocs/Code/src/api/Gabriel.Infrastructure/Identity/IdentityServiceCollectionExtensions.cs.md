# GabrielIdentityExtensions

> **File:** `src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs`  
> **Kind:** class

```csharp
public static class GabrielIdentityExtensions
```


GabrielIdentityExtensions serves as a centralized bootstrapper for Gabriel's authentication stack. Call AddIdentityAndAuth during startup to wire IdentityCore with the EF user store, register the JWT bearer authentication scheme, and expose token services, ensuring a single, consistent path for identity and token handling across the application.

## Remarks
This extension encapsulates the entire auth bootstrap behind a single entry point, reducing drift between identity, token minting/validation, and token storage. It binds JwtOptions from configuration and conditionally validates them at startup (subject to the SKIP_DB_INIT flag), while wiring IJwtTokenService, IRefreshTokenStore, and per-user preferences support. The JWT validation is explicit about issuer, audience, signing key, and lifetime, and the system uses a single JwtBearer scheme with a header-based flow and a HttpOnly access-cookie fallback for login scenarios.

## Notes
- If SKIP_DB_INIT is not set to "true", JwtOptions must be configured and validated at startup; specifically, SigningKey must be provided (and it must be at least 32 characters). This prevents runtime misconfiguration when the signing key is missing.
- The authentication pipeline is wired as a single JwtBearer scheme; the code path also notes a cookie-based writer on login, but the framework authentication remains JwtBearer, so mixing in other schemes requires careful coordination.
- The SigningKey, if provided, is converted to a SymmetricSecurityKey for token validation; an empty SigningKey results in a null IssuerSigningKey which will fail validation at runtime.
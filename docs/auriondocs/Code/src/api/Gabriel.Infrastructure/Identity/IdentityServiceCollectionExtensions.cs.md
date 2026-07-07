# GabrielIdentityExtensions

> **File:** `src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs`  
> **Kind:** class

```csharp
public static class GabrielIdentityExtensions
```


GabrielIdentityExtensions.AddIdentityAndAuth centralizes the app's identity and authentication setup. It wires IdentityCore with an EF-backed store for UserManager/SignInManager, registers token services and per-user preferences, and configures JwtBearer as the single authentication scheme that can read credentials from either the Authorization header or a HttpOnly login cookie. It also exposes the cookie names used by the cookie-backed flow to keep the JwtBearer and cookie-based paths aligned, ensuring a consistent authentication surface across the stack.

## Remarks
This extension serves as the single source of truth for the authentication stack: it binds JwtOptions from configuration, validates them at startup unless explicitly skipped, and wires a JwtTokenService along with a token store and per-user preferences into the DI container. By consolidating IdentityCore configuration and JWT-based authentication in one place, it prevents drift between the cookie writer, the JwtBearer flow, and token management, while supporting token rotation and theft detection via the IJwtTokenService.

## Notes
- Startup-time validation of JwtOptions is conditional on the SKIP_DB_INIT environment variable. If SKIP_DB_INIT is true, JwtOptions validation may be bypassed, so ensure configuration is correct by other means in that scenario.
- PasswordSignInAsync is not used in this setup; authentication relies on CheckPasswordSignInAsync for credential validation and JwtBearer (with a cookie fallback) for ongoing authentication, reflecting the design that there are no traditional Identity cookies.
- The SigningKey must be configured and have a minimum length (at least 32 characters). If the SigningKey is empty, IssuerSigningKey will be null and token validation will be ineffective or insecure.
- The AccessCookieName and RefreshCookieName constants are public to ensure all components—such as the JwtBearer cookie fallback and the AuthController cookie writer—consistently refer to the same cookie names.
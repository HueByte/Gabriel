# GabrielIdentityExtensions

> **File:** `src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs`  
> **Kind:** class

```csharp
public static class GabrielIdentityExtensions
```


GabrielIdentityExtensions provides a centralized extension method, AddIdentityAndAuth, to bootstrap IdentityCore-backed user management together with a JwtBearer-based authentication stack in ASP.NET Core apps. It serves as the single entry point to configure the Identity EF store, token services, per-user preferences, and the JWT bearer authentication pipeline, ensuring the cookie-based and bearer-based flows stay in sync via defined cookie names. Developers reach for this when wiring up authentication and user management in startup code instead of scattering configuration across multiple components.

## Remarks
This abstraction establishes a single source of truth for the authentication stack: IdentityCore with an EF store for user management, a token service for mint/refresh/revoke, and a JwtBearer scheme that can read tokens from the Authorization header or fall back to an HttpOnly cookie written by the AuthController. It also injects per-user preferences via a scoped service, ensuring user-specific choices are available alongside UserManager. The design intentionally avoids server-side UI scaffolding and password sign-in flows in favor of a token-centric model, with configuration guarded by environment-driven validation for build-time codegen scenarios.

## Notes
- SigningKey configuration is validated at startup (requiring a non-empty key of sufficient length) unless the SKIP_DB_INIT environment flag is set to "true". Misconfiguring the key will cause startup validation to fail.
- AccessCookieName and RefreshCookieName are defined here to prevent drift between the JwtBearer token reader and the AuthController's cookie writer; if you change them, align all usages accordingly.
- This extension configures a single authentication scheme (JwtBearer). If you need additional schemes or custom flows, configure them separately outside this method.
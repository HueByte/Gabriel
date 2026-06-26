# GabrielIdentityExtensions

> **File:** `src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs`  
> **Kind:** class

Registers and configures the application’s Identity + authentication stack. Use AddIdentityAndAuth when wiring up services in Program.cs/Startup to get IdentityCore (EF stores, UserManager/SignInManager), JWT bearer authentication with an HttpOnly access-cookie fallback, and the token/refresh infrastructure used by the app.

## Remarks
This class centralizes the authentication/identity configuration so callers don’t accidentally diverge in how tokens, cookies, and Identity are configured. It intentionally adds IdentityCore (no UI/cookie identity middleware) and configures JwtBearer as the single authentication scheme — the bearer handler reads Authorization: Bearer and falls back to an HttpOnly access cookie named by AccessCookieName. It also registers IJwtTokenService and IRefreshTokenStore for minting, refreshing and revoking tokens (the implementation handles refresh rotation and theft detection as noted in source comments), plus a scoped IUserPreferences service backed by ApplicationUser fields.

## Example
```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);
// configure IConfiguration as usual
builder.Services.AddIdentityAndAuth(builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

## Notes
- The constants AccessCookieName ("gabriel.access") and RefreshCookieName ("gabriel.refresh") are the single source of truth for cookie names; ensure any code that reads/writes auth cookies uses these values to stay in sync.
- JwtOptions are bound and validated at startup. The environment variable SKIP_DB_INIT="true" disables ValidateOnStart (used for build-time codegen scenarios); in normal runs ensure the signing key is present and meets the minimum length requirement.
- This extension configures JwtBearer as the only auth scheme — there are no Identity cookies enabled. Credential validation uses CheckPasswordSignInAsync / UserManager patterns rather than relying on cookie-based sign-in flows.
# GabrielIdentityExtensions

> **File:** `src/api/Gabriel.Infrastructure/Identity/IdentityServiceCollectionExtensions.cs`  
> **Kind:** class

Registers and configures the application's Identity and JWT-based authentication stack as a single, opinionated composition root. Call AddIdentityAndAuth during service registration (e.g. in Program.cs or Startup) when you want IdentityCore with an EF store for UserManager/SignInManager, a single JwtBearer authentication scheme (with an access-cookie fallback), and the project's token/refresh-token services wired up.

## Remarks
This class centralizes the app's authentication setup so the various pieces (JwtOptions binding and validation, IdentityCore + EF stores, IJwtTokenService/IRefreshTokenStore, and JwtBearer options) cannot drift apart. It intentionally uses JwtBearer as the only authentication scheme and treats Identity as the user management layer (password checks via SignInManager/UserManager) rather than relying on Identity cookies. The class was renamed from the framework extension to avoid a duplicate-symbol collision when consumers import both namespaces.

## Example
```csharp
// In Program.cs (minimal):
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddIdentityAndAuth(builder.Configuration);
var app = builder.Build();
// ... configure middleware and endpoints
app.Run();

// For build-time codegen where secrets aren't available you can skip startup-time
// JWT validation by setting SKIP_DB_INIT=true in the environment.
```

## Notes
- JwtOptions.SigningKey must be provided and sufficiently long (the code validates length); if missing the JWT validation pipeline will not be configured correctly. The SKIP_DB_INIT env var bypasses that validation for special build scenarios.
- This extension registers JwtBearer as the only authentication scheme; the app does not use Identity cookies for auth. Expect token-based auth (Authorization: Bearer) with a fallback to the HttpOnly access cookie that the app's AuthController sets.
- AccessCookieName and RefreshCookieName constants are exposed so cookie writers and the JWT fallback remain consistent.

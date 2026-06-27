# AuthController

> **File:** `src/api/Gabriel.API/Controllers/AuthController.cs`  
> **Kind:** class

A controller that exposes the application's authentication surface (register, login, refresh, logout, revoke, revoke-all, and me). Use this controller when you need a single HTTP API that supports both browser-based clients (which rely on HttpOnly cookies) and external/API clients (which consume JWTs from the response body). It centralizes user creation, credential verification, JWT issuance, refresh rotation, and refresh-token revocation.

## Remarks
This controller acts as the single entry point for authentication flows and intentionally serves two audiences with one API: it sets HttpOnly cookies so a browser-based webapp can rely on cookie transport while also returning JWTs in the response body for external clients that do not use cookies. It delegates user storage and credential checks to ASP.NET Identity (UserManager and SignInManager) and uses an IJwtTokenService to issue and rotate tokens. The registration endpoint respects a runtime-configurable kill-switch (AuthOptions.RegistrationEnabled) so registration can be disabled without restarting the app.

## Example
```csharp
// Browser client: POST JSON to /auth/login — browser receives HttpOnly cookies automatically; app ignores body tokens.
// External client: send POST JSON to /auth/login and read response body for access/refresh tokens.

using var client = new HttpClient { BaseAddress = new Uri("https://example.com/") };
var login = new { Email = "alice@example.com", Password = "s3cr3t" };
var resp = await client.PostAsJsonAsync("auth/login", login);
resp.EnsureSuccessStatusCode();
// External client reads tokens from response body
var jwt = await resp.Content.ReadFromJsonAsync<JwtResponse>();
```

## Notes
- Registration can be disabled at runtime via AuthOptions.RegistrationEnabled; the controller checks IOptionsMonitor on every request.
- Identity validation failures during register are surfaced as a DomainException (client receives a 400-style problem detailing Identity errors).
- Login deliberately returns the same error for "user not found" and "wrong password" to avoid account enumeration.
- The endpoints set HttpOnly cookies and also return tokens in the response body — browsers will rely on cookies (JS cannot read them), while API clients should use the body tokens.
- Revoke-all is protected ([Authorize]) and revokes every active refresh token for the current user; refresh can read the refresh token either from cookie or from the request body depending on client type.
# AuthController

> **File:** `src/api/Gabriel.API/Controllers/AuthController.cs`  
> **Kind:** class

*Figure: How AuthController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
AuthController["Incoming requests: POST /api/auth/register, login, refresh, logout, revoke, revoke-all; GET /api/auth/me"]

AuthController -->|"POST /api/auth/register"| RegisterRequest["RegisterRequest (email, password)"]
RegisterRequest -->|"check AuthOptions.RegistrationEnabled"| AuthOptions["AuthOptions: RegistrationEnabled?"]
AuthOptions -->|"disabled -> return 403 Problem"| Message["Message: Registration disabled (403)"]
AuthOptions -->|"enabled"| ApplicationUser["Create ApplicationUser and call UserManager.CreateAsync"]
ApplicationUser -->|"CreateAsync failed -> surface Identity errors"| DomainException["DomainException (validation errors -> 400)"]
ApplicationUser -->|"CreateAsync succeeded"| IJwtTokenService["IJwtTokenService.IssueAsync(userId,email) -> TokenPair"]

IJwtTokenService --> TokenPair["TokenPair (access, refresh)"]
TokenPair -->|"set HttpOnly cookies"| AuthCookies["AuthCookies.Set(Response, TokenPair)"]
TokenPair -->|"return tokens in body"| JwtResponse["JwtResponse (body tokens)"]

AuthController -->|"POST /api/auth/login"| LoginRequest["LoginRequest (email, password)"]
LoginRequest -->|"verify credentials"| IJwtTokenService
LoginRequest -->|"invalid -> 401"| Message["Message: Unauthorized (401)"]

AuthController -->|"POST /api/auth/refresh"| RefreshTokenRequest["RefreshTokenRequest (cookie OR body)" ]
RefreshTokenRequest -->|"read cookie OR body"| AuthCookies
AuthCookies -->|"extract refresh token"| IJwtTokenService
IJwtTokenService -->|"rotate refresh -> new TokenPair"| TokenPair
TokenPair -->|"set new cookies & return body"| JwtResponse

AuthController -->|"POST /api/auth/logout"| AuthCookies
AuthController -->|"revoke refresh from cookie"| IJwtTokenService
AuthController -->|"clear cookies"| AuthCookies
AuthController -->|"respond"| Message["Message: logged out / cookies cleared"]

AuthController -->|"POST /api/auth/revoke"| RefreshTokenRequest
RefreshTokenRequest -->|"revoke specific refresh token (body)"| IJwtTokenService
IJwtTokenService -->|"respond"| Message["Message: token revoked"]

AuthController -->|"POST /api/auth/revoke-all [Authorize]"| ICurrentUser["ICurrentUser: get current user id"]
ICurrentUser -->|"revoke every active refresh token for user"| IJwtTokenService
IJwtTokenService -->|"respond"| Message["Message: all tokens revoked"]

AuthController -->|"GET /api/auth/me [Authorize]"| ICurrentUser
ICurrentUser -->|"return id + email"| MeResponse["MeResponse (id, email)"]
```

```csharp
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
```


Exposes the application's authentication surface: register, login, refresh, logout, revoke (single token), revoke-all, and me. Use this controller when you need a single HTTP API that serves both browser-based clients (which rely on HttpOnly cookies) and external clients (which consume JWTs in the response body). Endpoints mint, rotate and revoke refresh tokens via the IJwtTokenService and set/clear HttpOnly cookies so the same endpoints work for both audiences.

## Remarks
This controller centralizes JWT issuance and refresh-token lifecycle management while delegating user persistence and credential checks to ASP.NET Identity (UserManager and SignInManager). It intentionally returns tokens in the response body and also writes HttpOnly cookies: the browser-based SPA relies on the cookies (ignoring the body), whereas external clients ignore cookies and use the tokens from the body. Registration is guarded by an AuthOptions toggle (read via IOptionsMonitor so it can be flipped at runtime without restarting the app). Identity validation failures are surfaced as a clean client error (password rules, duplicate email, etc.), and invalid credentials follow a unified unauthorized path to avoid account enumeration.

## Example
```csharp
// Browser flow (frontend): POST /api/auth/login; browser receives HttpOnly cookies automatically and the SPA ignores the JSON body.
// External client flow (CLI): POST /api/auth/login and read the JSON body for access/refresh tokens.

using var client = new HttpClient();
var login = new { Email = "alice@example.com", Password = "P@ssw0rd" };
var resp = await client.PostAsJsonAsync("/api/auth/login", login);
resp.EnsureSuccessStatusCode();
// External client: read tokens from the body
var jwt = await resp.Content.ReadFromJsonAsync<JwtResponse>();

// To refresh using cookie (browser): POST /api/auth/refresh with no body; browser attaches cookie automatically.
// To refresh for external client: POST /api/auth/refresh with { refreshToken: "..." } in the body and read new tokens from the body.
```

## Notes
- Registration can be disabled at runtime via AuthOptions. When disabled the controller returns a 403 Problem response.
- The controller sets HttpOnly cookies (not readable by JavaScript). External clients must use the tokens returned in the response body.
- Identity validation errors from CreateAsync are aggregated and surfaced as a domain/client error so callers receive a clean 400-style response rather than raw Identity details.

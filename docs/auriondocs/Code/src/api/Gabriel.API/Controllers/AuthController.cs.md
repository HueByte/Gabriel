# AuthController

> **File:** `src/api/Gabriel.API/Controllers/AuthController.cs`  
> **Kind:** class

*Figure: How AuthController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
Start["AuthController receives request"]
Start --> Route{"Route"}

Route -->|"/register"| RegisterReq["AuthController: POST /api/auth/register (RegisterRequest)"]
RegisterReq --> RegCheck["AuthOptions: RegistrationEnabled?"]
RegCheck -->|"No"| RegDisabled["Return Message 'Registration disabled' (403)"]
RegCheck -->|"Yes"| CreateUser["Create ApplicationUser; on failure throw DomainException"]
CreateUser --> IssueReg["IJwtTokenService: Issue TokenPair"]
IssueReg --> SetCookiesReg["AuthCookies: Set cookies from TokenPair"]
SetCookiesReg --> ReturnReg["Return JwtResponse (TokenPair)"]

Route -->|"/login"| LoginReq["AuthController: POST /api/auth/login (LoginRequest)"]
LoginReq --> Verify["Verify credentials"]
Verify -->|"Fail"| LoginFail["Return Message 'Unauthorized' (401)"]
Verify -->|"OK"| IssueLogin["IJwtTokenService: Issue TokenPair"]
IssueLogin --> SetCookiesLogin["AuthCookies: Set cookies from TokenPair"]
SetCookiesLogin --> ReturnLogin["Return JwtResponse (TokenPair)"]

Route -->|"/refresh"| RefreshReq["AuthController: POST /api/auth/refresh (RefreshTokenRequest)"]
RefreshReq --> ReadRefresh["Read RefreshToken from AuthCookies OR RefreshTokenRequest body"]
ReadRefresh --> Rotate["IJwtTokenService: Rotate/validate RefreshToken -> new TokenPair"]
Rotate --> SetCookiesRefresh["AuthCookies: Set new cookies from TokenPair"]
SetCookiesRefresh --> ReturnRefresh["Return JwtResponse (TokenPair)"]

Route -->|"/logout"| Logout["AuthController: POST /api/auth/logout"]
Logout --> ReadCookieLogout["AuthCookies: read RefreshToken"]
ReadCookieLogout --> RevokeLogout["Revoke that RefreshToken (RefreshToken)"]
RevokeLogout --> ClearCookies["AuthCookies: Clear cookies"]
ClearCookies --> ReturnLogout["Return Message 'Logged out'"]

Route -->|"/revoke"| Revoke["AuthController: POST /api/auth/revoke (RefreshTokenRequest)"]
Revoke --> RevokeSpecific["Revoke provided RefreshToken (RefreshToken)"]
RevokeSpecific --> ReturnRevoke["Return Message"]

Route -->|"/revoke-all"| RevokeAll["#quot;AuthController: POST /api/auth/revoke-all [Authorize"]"]
RevokeAll --> GetCurrent["ICurrentUser: get current user id"]
GetCurrent --> RevokeAllAction["Revoke all RefreshTokens for user"]
RevokeAllAction --> ReturnRevokeAll["Return Message"]

Route -->|"/me"| Me["#quot;AuthController: GET /api/auth/me [Authorize"]"]
Me --> GetCurrent2["ICurrentUser: get id + email"]
GetCurrent2 --> ReturnMe["Return MeResponse (id + email)"]
```

```csharp
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
```


A single HTTP authentication surface for the application that exposes endpoints for registration, login, refresh, logout, token revocation, and retrieving the current user. Use this controller when you need the canonical server-side implementation of authentication that serves both browser-based clients (via HttpOnly cookies) and external clients (via token JSON in the response body) from the same endpoints.

## Remarks
This controller centralizes JWT issuance, refresh rotation, cookie management, and Identity integration so the rest of the app can rely on a single, consistent auth surface. It delegates user persistence and credential checks to ASP.NET Identity (UserManager/SignInManager), issues JWT pairs through IJwtTokenService, and uses AuthCookies helpers to set/clear/read cookies. The design intentionally returns tokens in the response body while also setting HttpOnly cookies so the same endpoints work for both browser flows (which rely on cookies) and external API clients (which read tokens from the body).

## Notes
- Registration can be disabled at runtime via AuthOptions.RegistrationEnabled (checked through IOptionsMonitor), so flipping the flag takes effect immediately without an app restart.
- Endpoints that create or rotate tokens (register, login, refresh) both set cookies and return the token pair in the response body — browser clients should rely on cookies while external clients should use the body.
- The controller avoids account enumeration: a missing user and a bad password follow the same error path for login. Identity validation errors during registration are surfaced as a DomainException containing Identity's error descriptions.
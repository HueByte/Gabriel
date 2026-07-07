# AuthController

> **File:** `src/api/Gabriel.API/Controllers/AuthController.cs`  
> **Kind:** class

*Figure: How AuthController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
AuthController["AuthController: single auth surface (register/login/refresh/logout/revoke/revoke-all/me)"]
AuthOptions["AuthOptions: check RegistrationEnabled flag each request"]
RegisterRequest["RegisterRequest: body (email, password)"]
ApplicationUser["ApplicationUser: built and passed to UserManager.CreateAsync"]
DomainException["DomainException: thrown if user creation fails (surfaces 400)"]
IJwtTokenService["IJwtTokenService: IssueAsync / Rotate / Revoke refresh tokens"]
TokenPair["TokenPair: access + refresh tokens (set as cookies AND returned in body)"]
AuthCookies["AuthCookies: set or clear HttpOnly cookies"]
ICurrentUser["ICurrentUser: identifies current user for revoke-all"]
MeResponse["MeResponse: id + email returned by GET /me"]

AuthController -->|"POST /register with body"| RegisterRequest
AuthController -->|"check registration flag"| AuthOptions
AuthOptions -->|"disabled -> return 403 Problem"| AuthController

RegisterRequest -->|"construct user & CreateAsync"| ApplicationUser
ApplicationUser -->|"CreateAsync fails"| DomainException
ApplicationUser -->|"CreateAsync succeeds -> IssueAsync"| IJwtTokenService
IJwtTokenService -->|"issues TokenPair"| TokenPair
TokenPair -->|"set HttpOnly cookies"| AuthCookies
TokenPair -->|"returned in response body"| AuthController

AuthController -->|"POST /login (verify creds -> IssueAsync)"| IJwtTokenService
AuthController -->|"POST /refresh (reads cookie OR body -> rotate)"| IJwtTokenService
AuthController -->|"POST /logout (revoke refresh from cookie)"| IJwtTokenService
AuthController -->|"POST /logout -> clear cookies"| AuthCookies
AuthController -->|"POST /revoke (body token) -> revoke specific"| IJwtTokenService

AuthController -->|"POST /revoke-all [Authorize]"| ICurrentUser
ICurrentUser -->|"resolve user -> revoke all refresh tokens"| IJwtTokenService

AuthController -->|"GET /me [Authorize] -> return id+email"| MeResponse
```

```csharp
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
```


Exposes the application's authentication surface: endpoints for register, login, refresh, logout, revoke (single token), revoke-all (all tokens for the current user) and me (current user info). It is the single place clients—both browser-based and external—interact with account creation, credential verification, JWT issuance/rotation, and refresh-token revocation.

## Remarks
AuthController centralizes two concerns that often diverge: Identity-backed user management (creation, password verification and lockout) and JWT lifecycle (issue, rotate, revoke). To support both browser clients and external consumers, its register/login/refresh paths both set HttpOnly cookies and return token pairs in the response body so the web app can rely on cookies while API clients can use the body tokens. The controller delegates user operations to UserManager/SignInManager, token operations to IJwtTokenService, and cookie handling to AuthCookies; AuthOptions (via IOptionsMonitor) is used to toggle registration at runtime.

## Notes
- Registration can be disabled at runtime via AuthOptions. The controller checks IOptionsMonitor.AuthOptions.RegistrationEnabled on each request so flipping that flag takes effect without restarting the app.
- Register/Login/Refresh both set HttpOnly cookies and return tokens in the response body intentionally: browser flows rely on cookies, external clients ignore cookies and use the returned tokens. Clients must pick the mechanism appropriate for them.
- The controller intentionally avoids account-enumeration: missing user and wrong-password paths are handled the same (unauthorized). Identity creation errors (password rules, duplicate email, etc.) are surfaced as a DomainException to produce a clean 400-style error surface for callers.
# JwtTokenService

> **File:** `src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs`  
> **Kind:** class

*Figure: How JwtTokenService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
IJwtTokenService["Call: IssueAsync or RefreshAsync"]
IJwtTokenService --> JwtTokenService["EnsureConfigured; route to chosen method"]

subgraph Issue
  JwtTokenService --> JwtOptions["MintAccessJwt(userId, email) -> (access, accessExpires)"]
  JwtTokenService --> IRefreshTokenStore["PersistNewRefreshTokenAsync(userId) -> (refreshPlaintext, RefreshToken)"]
  IRefreshTokenStore --> IUnitOfWork["SaveChangesAsync() to commit refresh token (IssueAsync)"]
  IUnitOfWork --> TokenPair["Return TokenPair(access, accessExpires, refreshPlaintext, RefreshToken.ExpiresAt)"]
end

subgraph Refresh
  JwtTokenService -->|"if string.IsNullOrWhiteSpace(refreshToken) -> throw Unauthorized"| RefreshToken
  JwtTokenService -->|"HashToken(refreshToken) then FindByHashAsync(hash)"| IRefreshTokenStore
  IRefreshTokenStore --> RefreshToken["existing == null -> invalid (missing in DB); existing != null -> check revocation/expiry/rotation"]
  RefreshToken -->|"on valid rotation: mint access, rotate refresh, SaveChangesAsync, return TokenPair"| TokenPair
  RefreshToken -->|"on reuse detection within RotationGracePeriod -> treat as reuse/theft detection"| JwtTokenService
end

ApplicationUser["UserManager<ApplicationUser> stored in service (held but not shown in these paths)"]
```

```csharp
public class JwtTokenService : IJwtTokenService
```


Creates and rotates JWT-based authentication tokens (access + refresh) and enforces refresh-token hygiene.

Use this service when you need a single place to mint short-lived access JWTs and issue/rotate long-lived refresh tokens, with built-in replay/theft detection and persistence. It is the infrastructure implementation of IJwtTokenService used by higher-level authentication workflows (issue on login, refresh on cookie/API refresh requests, revoke on logout or compromise).

## Remarks
JwtTokenService centralizes JWT creation and refresh-token lifecycle management. It mints access tokens, stores refresh-token hashes in a persistent IRefreshTokenStore, and performs rotation on refresh requests so that each refresh produces a new refresh token while marking the previous one as replaced or revoked. A short RotationGracePeriod (5 minutes) is applied to tolerate benign races (multi-tab requests, delayed SSE responses) while still allowing reliably-detected reuse of stale tokens to be treated as a theft signal. The service depends on JwtOptions for configuration, a UnitOfWork to persist database changes, and ASP.NET Identity's UserManager when user data is required.

## Notes
- IssueAsync explicitly calls the unit-of-work SaveChangesAsync to commit the newly-created refresh-token row; without this save the refresh token may remain only in the EF change tracker and never persist, causing subsequent refresh attempts to fail as "not found."
- A missing refresh-token hash in the store results in an UnauthorizedAccessException and a logged warning that includes a short token-hash prefix — look for these logs when diagnosing lost Set-Cookie events, DB resets, or tampered tokens.
- RotationGracePeriod intentionally allows a small window (5 minutes) where a recently-rotated-but-still-sent token will not immediately trigger theft detection; reuse after that window is treated as a strong signal of token theft and leads to stricter revocation handling.
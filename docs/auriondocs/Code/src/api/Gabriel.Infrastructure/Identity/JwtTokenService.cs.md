# JwtTokenService

> **File:** `src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs`  
> **Kind:** class

*Figure: How JwtTokenService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
JwtTokenService["EnsureConfigured; handles IssueAsync or RefreshAsync"]
JwtOptions["JwtOptions: configuration read by EnsureConfigured"]
JwtTokenService --> JwtOptions

JwtTokenService --> RefreshToken["IssueAsync: Mint access JWT; PersistNewRefreshTokenAsync -> creates RefreshToken entity + plaintext"]
RefreshToken --> IUnitOfWork["IUnitOfWork.SaveChangesAsync commits refresh token row"]
IUnitOfWork --> TokenPair["IssueAsync returns TokenPair: access, accessExpires, refreshPlaintext, refreshExpires"]

JwtTokenService --> IRefreshTokenStore["RefreshAsync: HashToken(refreshToken); FindByHashAsync(hash)"]
IRefreshTokenStore --|"existing is null (not found)"| IJwtTokenService["Throw UnauthorizedAccessException (invalid or missing)"]
IRefreshTokenStore --|"existing found"| RefreshToken
```

```csharp
public class JwtTokenService : IJwtTokenService
```


Issues and validates JWT access tokens plus server-persisted, rotating refresh tokens. Use this service when you need refresh-token rotation, server-side storage of refresh tokens, and basic detection of token reuse/theft instead of relying on stateless refresh-only approaches.

## Remarks
This class coordinates JwtOptions, the persistent refresh-token store, the unit-of-work, and Identity's UserManager to mint access JWTs, create and persist refresh-token records, and validate/rotate refresh tokens on refresh requests. It implements defensive behaviors (hashing refresh tokens, logging lookup misses with a hash prefix, and a short "rotation grace period" to tolerate races from multi-tab browsers or long-lived SSE responses) to reduce false positives while still allowing detection of suspicious reuse.

## Example
```csharp
// Issue initial tokens (e.g. on login)
var tokenService = /* resolved from DI */;
var pair = await tokenService.IssueAsync(userId, userEmail, cancellationToken);
// pair.AccessToken, pair.AccessTokenExpiresAt, pair.RefreshToken, pair.RefreshTokenExpiresAt

// Refresh (e.g. endpoint that reads refresh token from cookie)
try
{
    var newPair = await tokenService.RefreshAsync(refreshTokenFromClient, cancellationToken);
    // return/set newPair to client
}
catch (UnauthorizedAccessException)
{
    // map to 401 Unauthorized and force re-login
}
```

## Notes
- IssueAsync explicitly commits the new refresh-token row via the unit-of-work; callers must await the call to ensure the persistent refresh token is stored before relying on it.
- RefreshAsync will throw UnauthorizedAccessException for missing or invalid refresh tokens; callers should translate that to an HTTP 401/unauthorized response.
- RotationGracePeriod (5 minutes) is intentionally permissive to absorb common multi-tab or in-flight-request races; reuse within that window may be tolerated while reuse well after it is considered a strong theft signal.
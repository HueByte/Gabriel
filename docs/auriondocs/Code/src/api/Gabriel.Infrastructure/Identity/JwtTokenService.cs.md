# JwtTokenService

> **File:** `src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs`  
> **Kind:** class

Issues and rotates JWT access + refresh tokens and encapsulates the token lifecycle (minting access JWTs, persisting refresh tokens, rotating on use, and detecting reuse/theft). Reach for JwtTokenService whenever you need an application-level, secure implementation of issuing and refreshing JWT/refresh-token pairs instead of handling token creation, hashing, persistence, rotation, and theft-detection manually.

## Remarks
This class centralizes the authentication token lifecycle and coordinates three collaborators: an IRefreshTokenStore for storing hashed refresh tokens, a Unit-of-Work to commit persistence, and ASP.NET Identity's UserManager for user-related operations. It enforces refresh-token rotation, keeps a short RotationGracePeriod to absorb benign races (multi-tab, in-flight requests, long SSE responses), and logs lookup misses and suspicious reuse to aid incident investigation. It also validates configuration before operating.

## Example
```csharp
// Issue a new pair for a freshly authenticated user
var pair = await jwtTokenService.IssueAsync(userId, userEmail, cancellationToken);
// pair.AccessToken, pair.AccessTokenExpiresAt, pair.RefreshTokenPlaintext, pair.RefreshTokenExpiresAt

// Later, when client presents the stored refresh token to get a new access token
try
{
    var newPair = await jwtTokenService.RefreshAsync(presentedRefreshToken, cancellationToken);
    // set cookie with newPair.RefreshTokenPlaintext and return newPair.AccessToken
}
catch (UnauthorizedAccessException ex)
{
    // treat as an authentication failure: force re-login, revoke sessions, or show login UI
}
```

## Notes
- RotationGracePeriod (5 minutes) is intentional: it tolerates normal browser timing races but is short enough to detect replayed/stolen tokens later.
- IssueAsync persists the refresh-token row and explicitly saves changes via the unit-of-work — omitting that commit will leave the token only in the EF change tracker and cause subsequent refresh attempts to fail.
- RefreshAsync throws UnauthorizedAccessException for missing, invalid, or revoked tokens; callers should map that to an authentication failure flow.
- The service logs a short prefix of the refresh-token hash on lookup misses to help triage recurring invalid-token patterns without logging full secrets.
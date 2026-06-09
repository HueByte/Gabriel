# JwtTokenService

> **File:** `src/api/Gabriel.Infrastructure/Identity/JwtTokenService.cs`  
> **Kind:** class

Issues and rotates JWT access and refresh tokens for ApplicationUser principals. Use this service when you need a complete token lifecycle implementation (minting access JWTs, persisting refresh tokens, rotating on refresh and detecting token reuse) rather than manually creating tokens or storing refresh rows yourself.

## Remarks
JwtTokenService ties together access-token minting, refresh-token persistence, and rotation logic. It delegates storage to IRefreshTokenStore and uses an IUnitOfWork to ensure the refresh token row is committed; access tokens are produced by an internal MintAccessJwt helper. A short RotationGracePeriod is applied to tolerate benign races (multi-tab requests, long-lived SSE responses) while still enabling strong theft-detection for replayed refresh tokens.

## Example
```csharp
// Issue a new pair when a user logs in
var pair = await jwtTokenService.IssueAsync(user.Id, user.Email, cancellationToken);
// pair.AccessToken, pair.AccessTokenExpiresAt, pair.RefreshTokenPlaintext, pair.RefreshTokenExpiresAt

// Later: client presents refresh cookie/plaintext to rotate
try
{
    var newPair = await jwtTokenService.RefreshAsync(presentedRefreshToken, cancellationToken);
    // return newPair to client and set cookie
}
catch (UnauthorizedAccessException)
{
    // refresh failed (invalid, revoked, or expired)
}
```

## Notes
- IssueAsync explicitly calls the injected IUnitOfWork.SaveChangesAsync to persist the new refresh-token row; omitting that commit would leave the token only in EF's change tracker and cause subsequent refresh attempts to fail.
- RefreshAsync expects a non-empty refresh token and throws UnauthorizedAccessException for missing/invalid values; callers should handle that to return appropriate HTTP responses.
- RotationGracePeriod (5 minutes) intentionally accepts short reuse of a recently rotated token to absorb benign client races; reuse beyond that window is treated as a stronger theft signal.
- The service logs a prefix of the token hash when a lookup misses to aid investigation without storing full secrets.
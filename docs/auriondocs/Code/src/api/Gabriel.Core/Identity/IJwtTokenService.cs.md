# IJwtTokenService.cs

> **Source:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`

## Contents

- [IJwtTokenService](#ijwttokenservice)
- [TokenPair](#tokenpair)

---

## IJwtTokenService

> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** interface

Issues short-lived JWT access tokens and long-lived, server-backed refresh tokens. Use this abstraction when you want stateless access tokens (validated by signature) together with rotatable, revocable refresh tokens so you can safely revoke or detect theft without making access tokens stateful.

## Remarks
This interface separates concerns between short-lived access tokens and long-lived refresh tokens: access tokens remain stateless for fast validation, while refresh tokens are stored and managed server-side so they can be rotated and revoked. RefreshAsync performs rotation and is expected to detect reuse (a theft signal) and trigger revocation of the refresh-token family for the affected user. The service is typically used by authentication endpoints (login, token refresh, sign-out) and integrates with secure client-side storage (HttpOnly cookies or secure stores) for refresh tokens.

## Example
```csharp
// During initial sign-in (session already validated):
TokenPair pair = await jwtService.IssueAsync(userId, email, ct);
// store pair.AccessToken in Authorization header for API calls
// store pair.RefreshToken in an HttpOnly, Secure cookie

// When access token expires, exchange refresh token for a new pair:
TokenPair newPair = await jwtService.RefreshAsync(refreshTokenFromCookie, ct);
// replace stored refresh token with newPair.RefreshToken and use newPair.AccessToken

// Sign out from single device:
await jwtService.RevokeAsync(refreshTokenFromCookie, ct);

// Sign out everywhere / after suspected compromise:
await jwtService.RevokeAllForUserAsync(userId, ct);
```

## Notes
- Treat refresh tokens as highly sensitive: store them in HttpOnly, Secure cookies or a similarly protected client storage; never expose them to JavaScript.
- Revoking refresh tokens does not immediately invalidate already-issued access tokens (they are stateless and valid until expiry); consider short access-token lifetimes or a token blacklist if immediate invalidation is required.
- Rotation introduces a concurrency pitfall: simultaneous refresh requests can look like reuse. Implementations must handle concurrent refresh attempts carefully to avoid false-positive theft detection and unintended revocations.

---

## TokenPair

> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** record

Represents a pair of JWT tokens — an access token and a refresh token — together with their expiration timestamps. Reach for this record when producing or returning both tokens from an authentication or token service so callers get the token strings and the exact times they expire.

## Remarks
This is a compact, immutable DTO (C# record) intended to be returned by JWT/token services (for example implementations of IJwtTokenService). It encodes both token values and their expiry instants as DateTimeOffset so callers can make correct decisions about refresh timing. Being a record, it provides value equality, deconstruction, and convenient copy-with semantics.

## Example
```csharp
// Constructing a TokenPair after issuing tokens
var accessExpires = DateTimeOffset.UtcNow.AddMinutes(15);
var refreshExpires = DateTimeOffset.UtcNow.AddDays(30);
var pair = new TokenPair(
    AccessToken: "eyJhbGci...",
    AccessExpiresAt: accessExpires,
    RefreshToken: "b1f2c3...",
    RefreshExpiresAt: refreshExpires);

// Deconstructing
var (access, accessExp, refresh, refreshExp) = pair;
Console.WriteLine($"Access expires at: {accessExp}");

// Returning from a service method
public TokenPair CreateTokens(User user)
{
    // ... generate tokens and compute expirations ...
    return pair;
}
```

## Notes
- Prefer using UTC (DateTimeOffset.UtcNow) for the expiry values to avoid timezone surprises when comparing instants.
- Treat token strings as sensitive secrets: do not log them in plain text and avoid including them in error messages.
- The record is a simple container — it does not perform validation or token generation itself; those responsibilities belong to the issuing service.

---
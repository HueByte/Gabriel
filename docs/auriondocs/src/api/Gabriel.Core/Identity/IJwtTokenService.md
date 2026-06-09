# IJwtTokenService.cs

> **Source:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`

## Contents

- [IJwtTokenService](#ijwttokenservice)
- [TokenPair](#tokenpair)

---

## IJwtTokenService

> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** interface

Issues short-lived JWT access tokens and server-side refresh tokens; use this interface when you want stateless, signature-validated access tokens together with rotatable refresh tokens that can be revoked or detected for reuse (theft). Prefer this service when you need immediate access validation via JWTs but still require the ability to terminate or rotate long-lived credentials.

## Remarks
This abstraction separates concerns: access tokens remain short-lived and validated purely by signature (no server state), while refresh tokens are stored and managed server-side so they can be revoked or rotated. Rotation lets the service replace a presented refresh token with a new one and detect reuse (which signals token theft), enabling an aggressive response such as revoking the entire refresh-token family for the user.

## Example
```csharp
// Issue after the user authenticated (e.g. cookie/session login)
var tokens = await jwtService.IssueAsync(userId, email, ct);
// send tokens.AccessToken to the client (Authorization: Bearer ...)
// store tokens.RefreshToken in a secure, HttpOnly cookie

// On access token expiry, client sends refresh token to obtain a new pair
try
{
    var newPair = await jwtService.RefreshAsync(currentRefreshToken, ct);
    // replace stored refresh token with newPair.RefreshToken and use newPair.AccessToken
}
catch (Exception ex)
{
    // handle invalid/used/expired refresh token — force re-authentication
}

// User signs out from one device
await jwtService.RevokeAsync(deviceRefreshToken, ct);

// User requests sign out from all devices or compromise detected
await jwtService.RevokeAllForUserAsync(userId, ct);
```

## Notes
- Treat refresh tokens as sensitive: store them in secure, HttpOnly cookies or equivalent secure storage on clients.
- Rotation implies the old refresh token is invalidated; reuse detection requires server-side state and careful concurrency handling.
- Access tokens are stateless and cannot be immediately revoked server-side — use short TTLs if you need quick invalidation.
- Protect the refresh endpoint (e.g. CSRF mitigations, rate-limiting) because it issues new access tokens.

---

## TokenPair

> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** record

Holds an issued access token and refresh token together with their respective expiration timestamps. Use this record when a token-issuing component (for example, an IJwtTokenService implementation) needs to return both tokens and their expiry times as a single immutable value.

## Remarks
This is a compact immutable record that groups the two tokens and their expirations; being a record it provides value-based equality, deconstruction, and pattern matching out of the box. It serves as a simple DTO between authentication/token services and callers (e.g., controllers or token stores).

## Example
```csharp
// Constructing a TokenPair after issuing tokens
var pair = new TokenPair(
    AccessToken: accessTokenString,
    AccessExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
    RefreshToken: refreshTokenString,
    RefreshExpiresAt: DateTimeOffset.UtcNow.AddDays(30));

// Deconstructing
var (access, accessExp, refresh, refreshExp) = pair;
```

## Notes
- Tokens are sensitive secrets; avoid logging them and store/transmit them securely.
- Expiration properties are DateTimeOffset values — account for offsets or normalize to a consistent clock when comparing or persisting.

---
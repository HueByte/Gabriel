# IJwtTokenService.cs

> **Source:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`

## Contents

- [IJwtTokenService](#ijwttokenservice)
- [TokenPair](#tokenpair)

---

## IJwtTokenService
> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** interface

```csharp
public interface IJwtTokenService
```


IJwtTokenService defines the contract for issuing and revoking JWT-based tokens used in the authentication workflow. It issues short-lived access tokens and long-lived, rotatable refresh tokens; access tokens are stateless and validated by signature, while refresh tokens are stored server-side to support revocation and theft detection. Typical usage: after a user authenticates, IssueAsync returns a TokenPair; use RefreshAsync to rotate tokens when needed; RevokeAsync to sign out from a single device; RevokeAllForUserAsync for sign-out-everywhere scenarios.

## Remarks
This abstraction separates token lifecycle concerns from business logic, enabling consistent security behavior across clients and services. It centralizes refresh-token rotation and theft handling; a reused or compromised token triggers a revoke-all for the user, limiting potential damage.

## Example
```csharp
// Common usage pattern
var tokens = await _jwtTokenService.IssueAsync(user.Id, user.Email, ct);

// Later, refresh on token rotation
var newTokens = await _jwtTokenService.RefreshAsync(tokens.RefreshToken, ct);

// Sign out from a single device
await _jwtTokenService.RevokeAsync(tokens.RefreshToken, ct);

// Sign out everywhere after credential compromise
await _jwtTokenService.RevokeAllForUserAsync(user.Id, ct);
```

## Notes
- Refresh tokens are rotated on each successful RefreshAsync call; reuse or theft of a refresh token is detected and triggers a revoke-all for the user.
- RevokeAsync invalidates a single refresh token (e.g., sign-out on one device); use RevokeAllForUserAsync to terminate all sessions for a user.
- TokenPair is the payload returned by issuance/refresh operations and should be treated as sensitive material; store and transmit it securely.


---

## TokenPair
> **File:** `src/api/Gabriel.Core/Identity/IJwtTokenService.cs`  
> **Kind:** record

```csharp
public record TokenPair(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `AccessToken` | `string` | — |
| `AccessExpiresAt` | `DateTimeOffset` | — |
| [`RefreshToken`](RefreshToken.cs.md) | `string` | — |
| `RefreshExpiresAt` | `DateTimeOffset` | — |


TokenPair is an immutable value object that groups an access token and its expiry alongside a refresh token and its expiry, allowing callers to transport both tokens and their lifetimes as a single unit. Use TokenPair when a token response provides both an access and a refresh token together, so you can pass around a single, strongly-typed value instead of separate strings and expiry dates.

## Remarks
- TokenPair uses C# record semantics to provide value-based equality; two pairs with identical fields compare equal and can be deconstructed in a pattern-based style.
- Being a record, it is effectively immutable: create a new TokenPair to reflect updated tokens or expiries rather than mutating an existing instance.
- It encapsulates related authentication data (tokens and their expiries) to reduce surface area and improve type-safety in authentication and token-refresh workflows.

## Example
```csharp
var tokenPair = new TokenPair(
    AccessToken: accessToken,
    AccessExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
    RefreshToken: refreshToken,
    RefreshExpiresAt: DateTimeOffset.UtcNow.AddDays(7)
);
```

## Notes
- Do not log or serialize tokens to logs or user-facing responses; treat them as sensitive data.
- Prefer UTC-based timestamps (DateTimeOffset with UTC) to avoid timezone-related bugs when comparing expiries.
- If you need to change any token or expiry, construct a new TokenPair instance; the existing one remains unchanged.

---
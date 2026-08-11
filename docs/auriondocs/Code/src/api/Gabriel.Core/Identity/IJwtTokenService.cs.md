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


IJwtTokenService provides a contract for issuing and managing JWT-based tokens: access tokens are stateless and validated by signature, while refresh tokens are server-side state that can be revoked. It issues an initial TokenPair for authenticated users, refreshes the pair with rotation and theft detection, and supports revoking single or all tokens across devices.

## Remarks
This abstraction separates stateless access tokens from stateful refresh tokens, enabling revocation and response to credential compromise without regenerating access keys. The refresh workflow rotates the refresh token on every use and, upon detecting token reuse, revokes the entire token family for the user to mitigate token theft. The design supports per-user session control across devices, including signing out a single device or signing out everywhere.

## Notes
- Token rotation relies on durable storage and atomic operations to prevent races when multiple clients refresh concurrently.
- RefreshAsync invalidates previously issued refresh tokens; treat old tokens as unusable after a successful refresh.
- IssueAsync requires the caller to be authenticated; userId and email are embedded into the issued TokenPair for binding to the user.

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


TokenPair is a lightweight value object that bundles an access token together with its expiration and a refresh token with its expiration. It serves as a single, immutable carrier for the complete token set commonly returned by authentication flows, enabling consumers to work with both tokens and their lifetimes in one place instead of juggling separate values.

## Remarks
TokenPair is declared as a record, which provides value-based equality and immutability. This makes it safe to pass through layers, cache, or reuse as a key without worrying about accidental mutation. By pairing the access and refresh tokens with their corresponding expiration times, it centralizes expiry logic and simplifies refresh scenarios: you always know when each token expires and can coordinate renewal accordingly. In practice, this type often represents the payload returned by a token service during login or token refresh operations, and is consumed by clients and services that need both tokens together.

## Example
```csharp
var tokenPair = new TokenPair(
    AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    AccessExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
    RefreshToken: "def50200ab34...",
    RefreshExpiresAt: DateTimeOffset.UtcNow.AddDays(7)
);
```

## Notes
- DateTimeOffset is used for expiry values to preserve offset information; ensure time sources are consistent (UTC is recommended) when comparing or validating expirations.
- TokenPair being a record means it is immutable; to derive a modified instance, use the with-expression (e.g., tokenPair with { AccessExpiresAt = newTime }).
- When serializing to JSON, DateTimeOffset values include offset information, so consumers should handle offset-aware timestamps accordingly.

---
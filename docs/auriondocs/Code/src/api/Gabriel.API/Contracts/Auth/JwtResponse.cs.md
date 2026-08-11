# JwtResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs`  
> **Kind:** record

```csharp
public record JwtResponse(
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
| [`RefreshToken`](../../../Gabriel.Core/Identity/RefreshToken.cs.md) | `string` | — |
| `RefreshExpiresAt` | `DateTimeOffset` | — |


JwtResponse is the data contract returned by the authentication endpoints /api/auth/jwt and /api/auth/jwt/refresh. It carries a short-lived AccessToken (a signed JWT) and its expiry, plus a RefreshToken (an opaque, high-entropy string) and its expiry. Clients should store the RefreshToken securely and exchange it for a new AccessToken via the refresh endpoint when the access token expires; on every refresh, the server rotates the RefreshToken to reduce the risk of token theft.

## Remarks
JwtResponse centralizes the token payload needed for continued authentication, separating token lifecycle from business logic. It makes clear that the AccessToken is used for API requests while the RefreshToken is used to obtain new access tokens without re-authenticating. The policy of rotating the refresh token on each refresh mitigates the risk of long-lived tokens being abused if a token is compromised.

## Example
```csharp
// Typical usage after a successful authentication
JwtResponse response = new JwtResponse(
    accessToken: accessToken,
    accessExpiresAt: DateTimeOffset.UtcNow.AddMinutes(15),
    refreshToken: refreshToken,
    refreshExpiresAt: DateTimeOffset.UtcNow.AddDays(7)
);

// Optional deconstruction for concise access
var (token, expires, rt, rtExpires) = response;
```

## Notes
- Treat the RefreshToken as highly sensitive; store it securely and avoid logging it or exposing it in analytics or error messages.
- Always use HTTPS for token transmission; the refresh flow rotates the refresh token to minimize the impact of potential leakage.
- The AccessToken is short-lived; decode it only to read claims for UI hints, but do not rely on client-side claims for authorization—server-side validation remains authoritative.
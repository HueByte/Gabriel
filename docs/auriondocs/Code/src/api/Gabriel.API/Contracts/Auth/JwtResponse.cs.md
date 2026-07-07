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


JwtResponse models the server payload returned by authentication endpoints that issue tokens. It encapsulates both the short-lived access token (AccessToken) and its expiration (AccessExpiresAt) as well as the opaque refresh token (RefreshToken) and its expiration (RefreshExpiresAt), enabling the client to refresh the access token without re-authenticating.

## Remarks

JwtResponse serves as a cohesive container for the two-token authentication handshake, pairing each token with an explicit expiration timestamp to simplify renewal flows and validation at the client boundary. The refresh token is rotated by the server on every refresh call, which helps mitigate token replay risks and strengthens ongoing session security. Clients should reuse the latest RefreshToken returned by the server and store tokens securely, aligning with the server's rotation behavior.

## Notes

- Do not log or expose tokens in plaintext or in client-side logs; store AccessToken and RefreshToken in secure locations and transmit them only over HTTPS.
- Treat the RefreshToken as a sensitive credential and rotate it on every refresh; reuse of an old RefreshToken is rejected by the server.
- Decode the AccessToken only to read claims for presentation or client-side UI decisions; do not rely on it for authorization checks without validating the signature on the server.
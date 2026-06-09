# JwtResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs`  
> **Kind:** record

```csharp
// Returned by POST /api/auth/jwt and POST /api/auth/jwt/refresh.
// Access token is a short-lived signed JWT (decode for claims).
// Refresh token is an opaque high-entropy string - store it server-side or in
// a secure client-side store, then trade it via /jwt/refresh when access expires.
// /refresh rotates the refresh token on every call.
public record JwtResponse(
    string AccessToken,
    DateTimeOffset AccessExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `claims` | `decode for` | — |


Represents the payload returned by the authentication endpoints that issue tokens: a short-lived signed access JWT and an opaque refresh token, each with their expiry timestamps. Use this type when returning issued tokens to clients (POST /api/auth/jwt and POST /api/auth/jwt/refresh) or when inspecting expiry times to decide when to refresh the access token.

## Remarks
This record groups the two-token pattern used by the API: an access token (a signed JWT suitable for decoding and extracting claims) and a refresh token (an opaque, high-entropy string that must be kept secret). The Authorization flow expects clients to use the AccessToken for authenticated requests until AccessExpiresAt, then call the refresh endpoint to exchange the RefreshToken for a new pair — the server rotates the refresh token on every successful refresh. AccessExpiresAt and RefreshExpiresAt are provided as DateTimeOffset so callers can compare against UtcNow to schedule refreshes or enforce expiration.

## Example
```csharp
// Creating a response to return from a controller after successful authentication
var accessToken = GenerateSignedJwt(claims); // signed JWT string
var refreshToken = GenerateOpaqueRefreshToken(); // high-entropy string stored server-side
var response = new JwtResponse(
    accessToken,
    DateTimeOffset.UtcNow.AddMinutes(15),   // access token lifetime
    refreshToken,
    DateTimeOffset.UtcNow.AddDays(30));     // refresh token lifetime

return Ok(response);

// Client-side: schedule a refresh shortly before AccessExpiresAt
if (DateTimeOffset.UtcNow >= response.AccessExpiresAt.AddSeconds(-30))
{
    // call POST /api/auth/jwt/refresh with response.RefreshToken
}
```

## Notes
- Treat the refresh token as sensitive: store it in a secure, HttpOnly cookie or a secure client store; do not expose it to third-party contexts or include it in URLs.
- Access tokens are JWTs and can be decoded to read claims, but their integrity must be validated server-side (signature and expiry). Do not assume the client clock is authoritative — compare expirations against server UtcNow where possible.
- The refresh endpoint rotates the refresh token on each successful call; clients must replace stored refresh tokens with the newly returned value.

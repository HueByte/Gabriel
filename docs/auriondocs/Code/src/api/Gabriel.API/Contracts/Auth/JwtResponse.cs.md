# JwtResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/JwtResponse.cs`  
> **Kind:** record

A small immutable DTO returned by the authentication endpoints when issuing tokens. Use this type to receive both the short-lived signed access token (JWT) and the longer-lived opaque refresh token together with their expiration timestamps — for example, as the response body from POST /api/auth/jwt and POST /api/auth/jwt/refresh.

## Remarks
This record cleanly separates the two-token model: an access token meant for frequent use and inspection (it is a JWT and can be decoded for claims), and a refresh token meant only for exchanging for new access tokens. The API rotates the refresh token on each refresh call, so clients must replace their stored refresh token with the returned value. Treat refresh tokens as high-value secrets and store them accordingly (server-side or a secure client-side store).

## Example
```csharp
// Deserialize a JwtResponse from an HTTP response (System.Text.Json)
using var response = await httpClient.PostAsync("/api/auth/jwt", content);
response.EnsureSuccessStatusCode();
var jwtResponse = await JsonSerializer.DeserializeAsync<JwtResponse>(await response.Content.ReadAsStreamAsync());

// Use the access token for an authorized request
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponse.AccessToken);

// When access token is expired or close to expiry, exchange the refresh token
// and replace stored refresh token with jwtResponse.RefreshToken from the refresh response.
```

## Notes
- Do not log token values or include them in unsecured storage or URLs.
- Account for clock skew when comparing AccessExpiresAt/RefreshExpiresAt to DateTimeOffset.UtcNow.
- Because refresh tokens are rotated on refresh, always update the stored refresh token atomically after a successful refresh call; losing the new token may require re-authentication.
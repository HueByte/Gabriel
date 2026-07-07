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


JwtResponse models the authentication response returned by POST /api/auth/jwt and POST /api/auth/jwt/refresh. It carries the short-lived access token (a signed JWT) and its expiration, plus a refresh token (an opaque high-entropy string) and its expiration, enabling the client to authorize requests and to obtain new tokens when the access token expires.

## Remarks
JwtResponse is a simple, immutable data contract that represents tokens exchanged during login and refresh flows. The access token is intended for bearer authentication and can be decoded to inspect claims, while the refresh token should be stored securely and rotated on every refresh to mitigate replay risks.

## Example
```csharp
// Example: after a successful login
JwtResponse tokens = await httpClient.PostAsJsonAsync("/api/auth/jwt", credentials)
                                   .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<JwtResponse>())
                                   .Unwrap();

// Use the access token for authenticated requests
httpClient.DefaultRequestHeaders.Authorization =
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens.AccessToken);

// Refresh when the access token expires
JwtResponse refreshed = await httpClient.PostAsJsonAsync("/jwt/refresh",
                                new { RefreshToken = tokens.RefreshToken })
                                      .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<JwtResponse>())
                                      .Unwrap();
```

## Notes
- Do not log or expose AccessToken or RefreshToken; treat tokens as sensitive information.
- AccessToken is short-lived; the refresh token is rotated on every refresh call to reduce replay risk; store the refresh token securely.
- If tokens are compromised, revoke and reissue tokens through the server-side authentication flow.
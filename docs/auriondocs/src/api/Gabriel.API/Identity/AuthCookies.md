# AuthCookies

> **File:** `src/api/Gabriel.API/Identity/AuthCookies.cs`  
> **Kind:** class

Sets and clears the application's authentication cookies (access and refresh) and provides a small accessor for reading the refresh cookie. Reach for this helper from authentication endpoints or middleware when you need to persist or remove TokenPair values in cookies using the same, safe defaults across the app.

## Remarks
This internal helper centralizes cookie behavior so all auth code uses identical names, paths and security flags. The access cookie is set for the site root and is intended to be read by the JwtBearer handler (via OnMessageReceived) so requests coming from the browser can produce an authenticated principal. The refresh cookie is intentionally scoped to the auth subtree (/api/auth) to avoid being sent with ordinary API calls and to reduce the blast radius if a response is leaked.

## Example
```csharp
// After issuing tokens in a login handler
var pair = new TokenPair { AccessToken = access, RefreshToken = refresh, AccessExpiresAt = accessExpiry, RefreshExpiresAt = refreshExpiry };
AuthCookies.Set(Response, pair);

// When logging out
AuthCookies.Clear(Response);

// When handling a refresh request inside /api/auth
var refreshToken = AuthCookies.ReadRefresh(Request);
if (refreshToken != null) { /* validate & rotate */ }
```

## Notes
- When deleting cookies the Path must match how the cookie was set; Clear uses Path = "/" for the access cookie and Path = "/api/auth" for the refresh cookie to match Set.
- Cookies are created HttpOnly and SameSite = Lax; JavaScript cannot read them and cross-site POSTs are not allowed by the SameSite setting.
- The Secure flag is set based on Request.IsHttps. In non-HTTPS (development) environments Secure will be false; in production use HTTPS so cookies are transmitted only over secure channels.
- ReadRefresh returns null if the refresh cookie is not present.

# AuthCookies

> **File:** `src/api/Gabriel.API/Identity/AuthCookies.cs`  
> **Kind:** class

```csharp
internal static class AuthCookies
```


AuthCookies centralizes the handling of the application's authentication cookies. It provides Set, Clear, and ReadRefresh helpers to persist, revoke, and retrieve token data in a consistent, secure manner, reducing the risk of misconfigured cookie options scattered across call sites.

## Remarks
AuthCookies encapsulates cookie semantics to enforce a uniform security posture and clear boundaries between access and refresh tokens. The access cookie is set with a site-wide path and standard security attributes, while the refresh cookie is intentionally scoped to /api/auth to limit exposure in the event of leaks. Deleting cookies relies on exact path values to ensure the browser actually removes them, which is why Clear uses the same path definitions as Set.

## Example
```csharp
// Typical usage after a successful login
AuthCookies.Set(HttpContext.Response, new TokenPair
{
    AccessToken = accessToken,
    AccessExpiresAt = accessExpiresAt,
    RefreshToken = refreshToken,
    RefreshExpiresAt = refreshExpiresAt
});
```

## Notes
- The refresh cookie is scoped to /api/auth, so it will only be sent with requests under that path. If a client interacts with endpoints outside this path, the refresh token will not be included automatically.
- When clearing cookies, use Clear to ensure both cookies are removed with their original paths; otherwise, the browser may retain stale cookies.

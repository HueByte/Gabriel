# AuthCookies

> **File:** `src/api/Gabriel.API/Identity/AuthCookies.cs`  
> **Kind:** class

```csharp
internal static class AuthCookies
```


AuthCookies centralizes the lifecycle of the authentication cookies used by the JWT-based flow. It exposes Set to issue both access and refresh cookies, Clear to revoke them, and ReadRefresh to read the refresh token from inbound requests, ensuring cookie handling is consistent across the app.

By aligning with GabrielIdentityExtensions' cookie names, it guarantees that the cookie presented by the browser maps to a valid principal during authentication.

## Remarks

By centralizing cookie policy decisions (HttpOnly, Secure, SameSite) and explicit paths, AuthCookies minimizes exposure if a response leaks. The refresh cookie is scoped to /api/auth to reduce risk on ordinary API calls. This class sits at the boundary between HttpResponse and the identity subsystem and should be the single place in code that mutates authentication cookies, simplifying testing and auditing.

## Notes

- Ensure the app's hosting environment reflects the true scheme for Secure cookies (IsHttps) — behind proxies TLS termination may affect this; configure forwarded headers accordingly.
- Deleting cookies requires matching the path and name; the code uses "/" for Access and "/api/auth" for Refresh, so ensure consistency when changing paths or cookie names.
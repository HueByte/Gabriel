# AuthCookies

> **File:** `src/api/Gabriel.API/Identity/AuthCookies.cs`  
> **Kind:** class

Manages setting, clearing and reading the authentication cookies used by the API (access and refresh tokens). Use this helper whenever you need to issue or remove the auth cookies in HTTP responses or read the refresh token from a request; it centralizes cookie names, options and path policies so callers do not duplicate cookie configuration.

## Remarks
AuthCookies keeps cookie handling consistent with the rest of the authentication stack: the access cookie is writable for the whole application and is intended to be read by the JwtBearer implementation (which inspects the cookie in OnMessageReceived), while the refresh cookie is intentionally scoped to a narrower path to limit its exposure. The class centralizes cookie options (HttpOnly, SameSite, Secure, Expires and Path) so token issuance and revocation behave predictably across the app.

## Notes
- When deleting a cookie, the Path provided must match the Path used when setting it; otherwise browsers may ignore the delete directive.
- The refresh cookie is set with Path = "/api/auth" so it will not be sent on ordinary API calls outside that subtree; ensure refresh endpoints are rooted under that path.
- Secure is determined from request.IsHttps; behind a proxy you must ensure the application sees the correct scheme (for example via forwarded headers) or Secure may not be set when required.
- ReadRefresh returns null if the refresh cookie is absent; callers should handle a null refresh token appropriately.
# AuthCookies

> **File:** `src/api/Gabriel.API/Identity/AuthCookies.cs`  
> **Kind:** class

```csharp
internal static class AuthCookies
```


AuthCookies is a small, internal helper that centralizes writing and clearing the authentication cookies used by the app's JWT-based flow. Use it whenever you need to persist or revoke the access and refresh tokens via cookies, guaranteeing consistent security attributes, cookie names sourced from GabrielIdentityExtensions, and scoped paths that minimize exposure.

## Remarks
AuthCookies encapsulates the cookie policy for authentication in one place, so callers don’t have to repeat HttpOnly/SameSite/Secure decisions. It ties the cookie lifetimes to the provided TokenPair and deliberately scopes the refresh cookie to the /api/auth path, reducing exposure if a response leaks. By aligning to GabrielIdentityExtensions' cookie names, it also ensures the identity system can rehydrate the principal when those cookies return to the browser.

## Notes
- Deleting cookies requires the same Path as when set; otherwise the browser ignores the deletion.
- The refresh cookie is scoped to /api/auth to minimize exposure and to ensure it isn't sent with ordinary API calls.
- The Secure flag is derived from the request's IsHttps; if TLS is terminated upstream, ensure the app sees HTTPS so cookies are marked as Secure and not transmitted over insecure connections.

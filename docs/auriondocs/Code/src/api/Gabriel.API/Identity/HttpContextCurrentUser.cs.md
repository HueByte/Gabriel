# HttpContextCurrentUser

> **File:** `src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs`  
> **Kind:** class

```csharp
public class HttpContextCurrentUser : ICurrentUser
```


HttpContextCurrentUser is a concrete ICurrentUser that reads the active user from IHttpContextAccessor.HttpContext.User. It normalizes information across the app’s three authentication schemes (Identity cookie, Identity opaque bearer, and our minted JWT) by inspecting the ClaimsPrincipal and exposing a simple, typed view: UserId as Guid?, IsAuthenticated as bool, and Email as string?. If the user is not authenticated, UserId is null; Email is null; and IsAuthenticated is false. The UserId is resolved by first looking for ClaimTypes.NameIdentifier, and if absent, the JWT 'sub' claim; the value is parsed as a Guid and returned if valid, otherwise null.

## Remarks
HttpContextCurrentUser provides a centralized, testable adapter over HttpContext.User that hides the details of claim naming across schemes. By exposing only UserId, IsAuthenticated, and Email, it reduces boilerplate in services and makes decisions about what constitutes the "current user" explicit and uniform.

Each consumer can rely on ICurrentUser without knowing whether the app uses cookie-based Identity, opaque bearer tokens, or JWTs minted by the API. The implementation also guards against missing or non-GUID claim values by returning nulls instead of throwing, letting callers handle unauthenticated or partially populated identities gracefully.

## Notes
- UserId will be null if the user is not authenticated or if the claims cannot be parsed as a GUID.
- Email may be null if the email claim is missing or not provided by the current authentication scheme.
- The class checks ClaimTypes.NameIdentifier and the "sub" claim to support both identity-based cookies and JWT-based tokens; add new claim names only if needed for additional schemes.
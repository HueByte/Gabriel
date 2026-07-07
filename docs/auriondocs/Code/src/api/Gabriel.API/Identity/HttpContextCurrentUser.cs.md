# HttpContextCurrentUser

> **File:** `src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs`  
> **Kind:** class

```csharp
public class HttpContextCurrentUser : ICurrentUser
```


HttpContextCurrentUser centralizes access to the currently authenticated user by reading from HttpContext.User and exposing a typed view of the key identity data. It supports Identity cookies, Identity opaque bearer tokens, and the app's minted JWT because all schemes populate the same ClaimsPrincipal; the user’s id is exposed as a nullable Guid via UserId, determined by NameIdentifier or sub. It also exposes IsAuthenticated and Email for convenient checks and metadata access without scattering HttpContext access logic throughout the codebase.

## Remarks
HttpContextCurrentUser abstracts away claim-name differences between schemes, providing a single, consistent source for UserId, IsAuthenticated, and Email. This makes testing easier by allowing code to depend on ICurrentUser rather than HttpContext, and it enables easier swapping of authentication strategies in the future. The properties gracefully handle the absence of HttpContext or unauthenticated requests.


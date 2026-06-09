# HttpContextCurrentUser

> **File:** `src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs`  
> **Kind:** class

Reads the current authenticated user from ASP.NET Core's HttpContext via an IHttpContextAccessor and exposes a small, testable ICurrentUser façade (UserId, IsAuthenticated, Email). Reach for this class when you need an application-level abstraction over ClaimsPrincipal so application code can depend on ICurrentUser instead of directly coupling to HttpContext or ClaimsPrincipal.

## Remarks
This implementation centralizes the logic for extracting user information from the request pipeline and normalizes differences between authentication schemes. It looks for the user identifier under ClaimTypes.NameIdentifier (used by ASP.NET Identity) and the JWT "sub" claim, and returns it only if it can be parsed as a Guid. The class intentionally tolerates a missing HttpContext (returns nulls) so callers can be used in contexts where there is no active HTTP request.

## Example
```csharp
// Startup / Program.cs registration
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Controller usage via DI
public class MyController : ControllerBase
{
    private readonly ICurrentUser _currentUser;

    public MyController(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public IActionResult Get()
    {
        if (!_currentUser.IsAuthenticated) return Unauthorized();
        var userId = _currentUser.UserId; // Guid? or null
        var email = _currentUser.Email;   // string? or null
        // ...
        return Ok();
    }
}
```

## Notes
- The UserId property expects the identifier claim to be a GUID; if the claim is missing or not a GUID the property returns null.
- HttpContext may be null (e.g., background services); the implementation returns null/false rather than throwing in those cases.
- Ensure AddHttpContextAccessor() is registered in DI; this class depends on IHttpContextAccessor being available.
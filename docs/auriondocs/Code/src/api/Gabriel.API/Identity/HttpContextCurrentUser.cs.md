# HttpContextCurrentUser

> **File:** `src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs`  
> **Kind:** class

Provides access to the current HTTP request's authenticated user by reading the ClaimsPrincipal from IHttpContextAccessor and exposing a minimal ICurrentUser surface (UserId, IsAuthenticated, Email). Use this when application services or controllers need a simple, testable way to obtain the current user's id or email without depending directly on HttpContext or ClaimsPrincipal.

## Remarks
This class adapts IHttpContextAccessor into the ICurrentUser abstraction so application code can depend on a small interface instead of HttpContext. It accounts for multiple authentication schemes by checking both ClaimTypes.NameIdentifier (used by ASP.NET Identity) and the JWT "sub" claim; it parses the claim value as a GUID and returns null if missing or invalid. It intentionally tolerates a missing HttpContext or unauthenticated requests and surfaces that via nullable properties.

## Example
```csharp
// Startup / Program.cs: register dependencies
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Consuming service
public class MyService
{
    private readonly ICurrentUser _currentUser;
    public MyService(ICurrentUser currentUser) => _currentUser = currentUser;

    public void DoSomething()
    {
        if (!_currentUser.IsAuthenticated) return; // no user
        var userId = _currentUser.UserId; // Guid? — null if not present or invalid
        var email = _currentUser.Email;   // string? — null if not present
        // ...business logic
    }
}
```

## Notes
- Properties return null when there is no HttpContext (e.g., background work) or the user is not authenticated.
- UserId will be null if the name identifier/sub claim is missing or not a parsable GUID; do not assume it is always present.
- This adapter does not perform authorization checks — it only reads claim values. Ensure callers enforce required permissions where appropriate.
# ICurrentUser

> **File:** `src/api/Gabriel.Core/Identity/ICurrentUser.cs`  
> **Kind:** interface

Represents the current request's authenticated user in the Core layer without depending on HTTP-specific types. Use this interface when code needs to read the caller's identity (ID, authentication status, email) but should remain independent of HttpContext/ASP.NET Core plumbing.

## Remarks
This interface exists to decouple core services from web-framework details: the API layer implements ICurrentUser (for example, by reading HttpContext.User) and registers it for dependency injection, while core services take a dependency on the abstraction. That keeps Core testable and portable and prevents a direct dependency on HttpContext or ClaimsPrincipal.

## Example
```csharp
public class OrderService
{
    private readonly ICurrentUser _currentUser;

    public OrderService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void PlaceOrder(Order order)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var userId = _currentUser.UserId.Value; // safe after IsAuthenticated check
        // Associate order with userId and continue...
    }
}
```

## Notes
- UserId and Email are nullable; check IsAuthenticated (or for null) before using UserId.Value or assuming Email is present.
- This abstraction intentionally exposes only a small subset of identity data; if you need roles or other claims, extend the interface or inject a claims-aware abstraction instead.
- ICurrentUser is per-request scope — do not cache or reuse instances beyond the request lifetime.
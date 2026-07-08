# HttpContextCurrentUser

> **File:** `src/api/Gabriel.API/Identity/HttpContextCurrentUser.cs`  
> **Kind:** class

```csharp
public class HttpContextCurrentUser : ICurrentUser
```


HttpContextCurrentUser is an ICurrentUser implementation that reads the current user from HttpContext.User. It works across Identity cookie, Identity opaque bearer, and minted JWTs because they all populate the same ClaimsPrincipal. It exposes three properties: UserId (Guid?), IsAuthenticated (bool), and Email (string?). UserId reads the user id from the NameIdentifier claim (ClaimTypes.NameIdentifier) or the "sub" claim used by our JWTs, parsing it as a Guid; if no suitable claim is found or parsing fails, it returns null. IsAuthenticated reflects whether the current HttpContext.User.Identity is authenticated. Email returns the Email claim value when present.

## Remarks
The adapter centralizes access to the authenticated user so business logic can depend on ICurrentUser rather than HttpContext details or specific authentication schemes. It decouples identity extraction from callers, making it easier to test and to swap authentication implementations. When data is missing (e.g., anonymous users or absent claims), the properties expose nullable results to encourage callers to handle anonymous or partial identities gracefully.

## Example
```csharp
// Example usage in a consuming service
public class DeliveryService
{
    private readonly ICurrentUser _currentUser;

    public DeliveryService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void ShowIdentity()
    {
        if (_currentUser.IsAuthenticated)
        {
            Console.WriteLine($"UserId={_currentUser.UserId}, Email={_currentUser.Email}");
        }
        else
        {
            Console.WriteLine("Anonymous user");
        }
    }
}
```

## Notes
- UserId is null when the user is not authenticated or the claims do not contain a valid GUID. 
- Email may be null if the Email claim is not present. 
- This class depends on IHttpContextAccessor; ensure it is registered in DI and available in non-controller contexts (or mock ICurrentUser in tests) to enable deterministic behavior.
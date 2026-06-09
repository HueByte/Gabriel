# ICurrentUser

> **File:** `src/api/Gabriel.Core/Identity/ICurrentUser.cs`  
> **Kind:** interface

A lightweight, per-request read-only view of the currently authenticated user. Use this interface when application core code needs identity information (user id, email, or whether the request is authenticated) without taking a dependency on ASP.NET Core types such as HttpContext or ClaimsPrincipal.

## Remarks
This abstraction exists to keep HTTP plumbing out of core logic: the API layer provides an implementation that reads from HttpContext.User and maps claims to these simple properties, while services in the core consume ICurrentUser. The properties are intentionally nullable to represent unauthenticated requests: UserId and Email may be null when IsAuthenticated is false.

## Example
```csharp
public class MyService
{
    private readonly ICurrentUser _currentUser;

    public MyService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void DoWork()
    {
        if (!_currentUser.IsAuthenticated)
        {
            // handle anonymous request or throw
            return;
        }

        Guid userId = _currentUser.UserId!.Value; // safe because IsAuthenticated was true
        string? email = _currentUser.Email; // may still be null if not present in claims

        // perform work on behalf of userId
    }
}
```

## Notes
- UserId and Email are nullable; always check IsAuthenticated or test for null before using them.
- This interface represents per-request state. Do not cache an implementation instance beyond the request lifetime.
- The interface does not perform authorization; it only exposes identity data. Enforcement of permissions must be handled elsewhere.
# LoginRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs`  
> **Kind:** record

Represents credentials submitted by a client to authenticate: an email address and a password. Use this record as the request DTO for login endpoints or any API surface that accepts user credentials.

## Remarks
This is a simple immutable data contract (positional record) intended for transport/model-binding scenarios in the API layer. It carries no validation, hashing, or security logic — those responsibilities belong to the authentication service or controller handling the request.

## Example
```csharp
// Model binding in an ASP.NET Core controller action
[HttpPost("/login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // pass request.Email and request.Password to your authentication service
    var result = _authService.Authenticate(request.Email, request.Password);
    return result.Success ? Ok(result) : Unauthorized();
}

// Manual construction
var req = new LoginRequest("user@example.com", "s3cr3tP@ss");
```

## Notes
- The record's default ToString() prints property values (including Password). Avoid logging or serializing instances with logs that include ToString().
- Password is stored as a plain string in memory; minimize its lifetime and avoid persisting it. Consider secure handling patterns if required by your threat model.
- No built-in validation: callers should validate email format and password requirements before use.
- As a record, it implements value-based equality and supports deconstruction (e.g., var (email, password) = request).
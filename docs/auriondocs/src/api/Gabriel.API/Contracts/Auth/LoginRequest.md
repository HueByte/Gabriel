# LoginRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs`  
> **Kind:** record

Represents the payload for an authentication (login) request containing an Email and Password. Use this record as the API contract or DTO when accepting credential data (for example, model-binding the body of a POST /auth/login request) so the two related values are passed together as a single, immutable value object.

## Remarks
This is a positional record, so it provides value-based equality, deconstruction, and supports with-expressions to produce modified copies. It's intended as a short-lived transfer object for incoming requests (model binding in ASP.NET Core will populate its properties from JSON/form data). It is not intended for long-term storage of credentials.

## Example
```csharp
// Typical model-binding usage in an ASP.NET Core controller action
[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest request)
{
    // Validate and pass to authentication service
    authService.SignIn(request.Email, request.Password);
    return Ok();
}

// Creating and working with the record directly
var req = new LoginRequest("user@example.com", "s3cr3t");
var (email, password) = req; // deconstruction
var updated = req with { Password = "n3wp@ss" }; // creates a copy with a different password
```

## Notes
- Treat Password as sensitive: do not log, include in exception messages, or persist it in plaintext. Strings are immutable and may remain in memory longer than expected; for high-security scenarios consider secure handling strategies.
- Because this is a record, two instances with the same Email and Password compare equal (value equality).
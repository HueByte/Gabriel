# RegisterRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RegisterRequest.cs`  
> **Kind:** record

A minimal data contract representing the input required to register a new user: an email address and a password. Use this record as the request DTO for a registration endpoint or any API surface that accepts new-account data.

## Remarks
This positional record is an immutable, value-oriented DTO intended for transport and model-binding (for example, as the body of an HTTP POST to an auth/register endpoint). It is not a domain entity and does not perform validation, hashing, or any security-related processing — those responsibilities belong to downstream services or validators.

## Example
```csharp
// Model-binding in an ASP.NET controller
[HttpPost("register")]
public IActionResult Register([FromBody] RegisterRequest request)
{
    // Validate and process request.Email and request.Password
    // Do NOT log request.Password
    return Ok();
}

// Creating an instance directly
var req = new RegisterRequest("alice@example.com", "P@ssw0rd!");
```

## Notes
- This record carries the raw password; avoid logging or persisting it in plaintext and ensure it is validated and hashed before storage.
- No built-in validation: call validators (DataAnnotations, FluentValidation, etc.) or perform explicit checks before use.
- Being a record, instances use value-based equality and expose init-only properties generated from the positional parameters.
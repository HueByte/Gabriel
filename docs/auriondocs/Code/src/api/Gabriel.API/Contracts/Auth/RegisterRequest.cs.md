# RegisterRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RegisterRequest.cs`  
> **Kind:** record

A small immutable data transfer object that represents the payload required to register a new user: an Email and a Password. Use this type as the request contract for registration endpoints or any API surface that needs to accept user credentials for account creation.

## Remarks
This is a positional C# record, so it provides value-based equality, deconstruction, and immutable init-only properties. It is intentionally a plain contract (no validation, hashing, or security behavior). Validation, password-strength checks, hashing, and storage responsibilities belong to the service or domain layers that consume this contract.

## Example
```csharp
// Model binding in an ASP.NET Core controller
[HttpPost("register")]
public IActionResult Register([FromBody] RegisterRequest request)
{
    // Validate request.Email and request.Password, hash the password,
    // then create the user via your user service/manager.
    return Ok();
}

// Direct construction
var req = new RegisterRequest("alice@example.com", "P@ssw0rd123");
```

## Notes
- The Password property contains the raw password in memory; avoid logging or persisting this object directly.
- Always transmit this payload over TLS and perform server-side validation and secure hashing before storing credentials.
- As a record, instances are immutable and support value equality and deconstruction (e.g., `var (email, password) = req;`).
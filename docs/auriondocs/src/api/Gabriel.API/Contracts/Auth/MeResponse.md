# MeResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/MeResponse.cs`  
> **Kind:** record

A lightweight data contract representing the authenticated user's identity returned by a "me" API endpoint. Use this record when you need to expose only the minimal, public information about the currently authenticated user (their Id and Email) rather than a full user domain model.

## Remarks
This is a positional C# record intended as an immutable DTO for API responses. As a record it provides value-based equality, a deconstruct method, and support for with-expressions, making it convenient for tests and transformations while keeping intent explicit: the structure is for transport, not business logic.

## Example
```csharp
// Creating and returning the response from a controller action
[HttpGet("me")]
public IActionResult Me()
{
    var user = GetCurrentUser(); // domain/user lookup
    var response = new MeResponse(user.Id, user.Email);
    return Ok(response);
}

// Deconstruction and with-expression
var me = new MeResponse(Guid.NewGuid(), "alice@example.com");
var (id, email) = me; // deconstruct
var updated = me with { Email = "alice@newdomain.com" }; // creates a new record
```

## Notes
- Records are immutable by design: modifications create new instances (with-expression) rather than mutating the existing one.
- Value-based equality means two MeResponse instances with the same Id and Email are considered equal.
- JSON property names and casing are controlled by the serializer configuration (e.g., System.Text.Json's camel-casing in ASP.NET Core); if you rely on exact property names, apply explicit attributes or configure the serializer accordingly.

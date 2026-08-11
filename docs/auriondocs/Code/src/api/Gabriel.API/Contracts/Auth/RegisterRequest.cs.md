# RegisterRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RegisterRequest.cs`  
> **Kind:** record

```csharp
public record RegisterRequest(string Email, string Password)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Email` | `string` | — |
| `Password` | `string` | — |


Represents the input for a user registration operation. This immutable record serves as the API contract that carries the user's Email and Password from the client to the server, typically deserialized from JSON in HTTP requests and passed to authentication logic.

## Remarks
Using a record with positional parameters enforces that Email and Password are provided at construction time, giving a clear, strongly-typed surface for registration data. As a public record, it benefits from value-based equality and concise construction, making it convenient to compare, test, and compose in pipelines. It is intended as a transport object; validation (format, password strength) should occur in higher layers before processing.

## Example
```csharp
var request = new RegisterRequest("alice@example.com", "P@ssw0rd!");
```

## Notes
- The default ToString on a record includes the Password value; avoid logging or displaying it. Redact or override ToString as needed; treat this object as sensitive until validated.
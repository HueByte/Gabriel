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


RegisterRequest is a lightweight, immutable data carrier used to transport a user’s registration credentials—Email and Password—from the client to the server. As a C# record with positional parameters, it benefits from value-based equality, concise construction, and easy deconstruction for downstream validation and serialization.

## Remarks
By representing the registration payload as a dedicated symbol, this type defines a stable API contract for the authentication flow. Its immutability and value-based equality simplify equality comparisons, pattern matching, and passing instances through layers without concern for accidental mutation. The positional parameters (Email, Password) make the required fields explicit and map naturally to common API field names during serialization.

## Notes
- Passwords are sensitive; avoid logging them or exposing them in UI debug traces. Consider redaction or custom serialization when writing to logs.
- This is a plain data transfer object; validation and business rules belong elsewhere (e.g., a service or validator) to keep concerns separated.
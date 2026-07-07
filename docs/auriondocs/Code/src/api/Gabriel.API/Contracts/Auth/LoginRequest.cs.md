# LoginRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/LoginRequest.cs`  
> **Kind:** record

```csharp
public record LoginRequest(string Email, string Password)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Email` | `string` | — |
| `Password` | `string` | — |


LoginRequest is an immutable data transfer object that carries the user's credentials for an authentication request. As a positional record with Email and Password, it provides a concise, value-based representation of the login payload sent to Gabriel.API's authentication endpoints.

## Remarks
LoginRequest defines the contract for the login operation: it gathers the input required to authenticate a user and keeps it as a simple, transport-agnostic data carrier. Its record nature enables value-based equality and convenient deconstruction, which helps when composing tests or extracting fields in client code before making the API call.

## Notes
- Treat Password as sensitive; redact it in logs and diagnostics.
- Ensure transport security (e.g., TLS) when sending this payload to the authentication service.
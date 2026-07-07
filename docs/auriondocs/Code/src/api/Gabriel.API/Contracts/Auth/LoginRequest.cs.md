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


LoginRequest is a lightweight, immutable data carrier that carries user credentials to an authentication endpoint. It bundles an Email and a Password into a single payload, providing a consistent contract for login operations instead of passing separate values around.

As a C# record, it benefits from value-based equality and straightforward construction, making it a natural transport contract for login calls.

## Remarks
LoginRequest serves as the canonical contract for login operations; it isolates the credentials payload and enables consistent validation, serialization, and auditing of login requests. Because it is a record, it provides value-based equality and immutability by default, which helps avoid unintended mutation and simplifies tests and comparisons. It pairs naturally with model-binding and serialization frameworks in typical API projects.

## Example
```csharp
var request = new LoginRequest("user@example.com", "P@ssw0rd!");
```

## Notes
- Treat Password as sensitive data; avoid logging or exposing it in error messages. Mask or omit the value in logs and UI wherever feasible.
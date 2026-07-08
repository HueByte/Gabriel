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


LoginRequest is an immutable data transfer object used to carry the credentials required for authentication. It encapsulates Email and Password into a single payload that can be sent to an authentication endpoint or service; using a record provides value-based equality and concise construction with the two required fields.

## Remarks

LoginRequest acts as the canonical payload for login operations. By representing credentials as a value object, it enables straightforward comparison and reliable transport of the Email/Password pair across service boundaries. As with any credential data, avoid logging the Password or persisting it in plaintext; ensure transmission occurs over a secure channel and consider validating inputs at the boundary where the object is created.

## Notes

- Do not log or display the Password; treat it as sensitive data.
- If you need validation, consider a custom constructor or a separate validator to enforce non-empty Email and Password before usage.
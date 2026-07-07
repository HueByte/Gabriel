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


RegisterRequest is a simple data transfer object that defines the payload for a user registration operation. It contains two pieces of information: Email and Password. As a C# record, it provides value-based equality and immutable semantics, making it a reliable API contract that can be deconstructed or passed through the stack without risk of mutation.

## Remarks
Acts as a boundary between the API surface and the authentication/domain layer, decoupling external input from domain models. By keeping input in a dedicated record, the system can evolve the API contract (e.g., add fields or change validation) without touching domain entities. The record's built-in deconstruction supports concise handlers and mapping to service calls.

## Notes
- Do not log the Password; treat it as sensitive information.
- Validation is not included here; implement validation elsewhere (e.g., data annotations, middleware, or service layer) to keep concerns separated.
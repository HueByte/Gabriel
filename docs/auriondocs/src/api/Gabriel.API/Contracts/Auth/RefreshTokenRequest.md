# RefreshTokenRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs`  
> **Kind:** record

Represents the request payload used to refresh authentication tokens. Use this record when sending a refresh-token to an authentication endpoint or service that issues new access/refresh credentials.

## Remarks
This is a positional C# record that provides a compact, immutable DTO with a single init-only property (RefreshToken). It is intended for API contracts or service method parameters where value-based equality and deconstruction are convenient. The type does not perform validation; callers or model binders should validate the token string before use.

## Example
```csharp
// As an API action parameter
[HttpPost("refresh")]
public IActionResult Refresh([FromBody] RefreshTokenRequest request)
{
    var tokens = _authService.Refresh(request.RefreshToken);
    return Ok(tokens);
}

// Simple construction and use
var req = new RefreshTokenRequest("eyJhbGciOiJI...refreshToken...");
await authClient.RefreshAsync(req);
```

## Notes
- The record is positional, so the compiler generates an init-only property named RefreshToken and value-based equality semantics.
- No validation or trimming is performed; null/empty or malformed tokens must be handled by the caller or model validation.
- JSON property naming depends on the serializer configuration (e.g., System.Text.Json defaults to the exact property name unless a naming policy like camelCase is configured).
# RefreshTokenRequest

> **File:** `src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs`  
> **Kind:** record

```csharp
public record RefreshTokenRequest(string RefreshToken)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`RefreshToken`](../../../Gabriel.Core/Identity/RefreshToken.cs.md) | `string` | — |


RefreshTokenRequest is a compact, immutable data carrier used to exchange a valid refresh token for a new access token. It carries a single piece of data, RefreshToken, and is sent as the request body to the authentication endpoint when the client needs to obtain fresh credentials without re-authenticating.

## Remarks
The record’s straightforward shape makes it a clear contract between client and server for token renewal; immutability helps prevent accidental tampering after construction. By using a positional record, the RefreshToken value is bound to construction, ensuring the payload represents a single, atomic refresh operation.

## Example
```csharp
var request = new RefreshTokenRequest("sample-refresh-token");
// Typically: serialize to JSON and POST to /api/auth/refresh
```

## Notes
- Treat the RefreshToken as sensitive data; avoid logging or displaying it in UI errors.
- Ensure the token is transmitted over HTTPS and that the server validates its integrity and expiry.
- If the API rotates refresh tokens on use, do not reuse a token after a successful refresh; store and use the new token instead.
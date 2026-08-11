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


Represents a request payload for refreshing an authentication token. This record is a lightweight, immutable value object that carries a single string property (RefreshToken) used by the authorization flow to obtain a new access token from the server. Because it is a C# record with a primary constructor, it benefits from value-based equality and concise construction, and it can be serialized as a JSON payload when calling the refresh endpoint.

## Remarks
Acts as a transport contract between the client and the authentication service, encapsulating the refresh token rather than passing a raw value through layers. The record-style shape gives you a strongly-typed, easily testable artifact with built-in value equality, making it straightforward to compare requests in tests or cache lookups. If future API evolution adds additional fields (e.g., device info or token metadata), they can be added here without changing the consumer's call sites.

## Example
```csharp
var request = new RefreshTokenRequest("sample_refresh_token");
```

## Notes
- Treat the RefreshToken as sensitive data; avoid logging, caching, or exposing it in client telemetry or logs.
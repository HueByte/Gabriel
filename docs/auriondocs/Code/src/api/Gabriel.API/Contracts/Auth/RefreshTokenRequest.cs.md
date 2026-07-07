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


Represents the request payload that carries a refresh token used to obtain a new access token. Use this contract when calling the authentication API's token refresh endpoint to exchange a valid refresh token for fresh credentials.

## Remarks
This is an immutable value object defined as a C# record. Its identity is determined by the RefreshToken value, and it benefits from value-based equality and deconstruction that records provide. It participates at the boundary between the client and the authentication service, keeping transport concerns separate from implementation details of token storage.

## Notes
- The property is read-only; to produce a modified copy, use the with-expression on the record (e.g., `var updated = request with { RefreshToken = newToken };`).
- Treat the refresh token as sensitive data: avoid logging it or exposing it in UI, and ensure it is transmitted and stored securely (prefer HTTPS and proper token storage policies).

## Source Code
```csharp
public record RefreshTokenRequest(string RefreshToken);
```

## Symbol To Document
- Name: RefreshTokenRequest
- Kind: record
- File: src/api/Gabriel.API/Contracts/Auth/RefreshTokenRequest.cs
- Language: csharp
- ID: 28403b76-2545-40a5-ba4f-123163de5aa8
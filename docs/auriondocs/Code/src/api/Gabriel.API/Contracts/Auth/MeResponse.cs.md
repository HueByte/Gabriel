# MeResponse

> **File:** `src/api/Gabriel.API/Contracts/Auth/MeResponse.cs`  
> **Kind:** record

```csharp
public record MeResponse(Guid Id, string Email)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| `Email` | `string` | — |


MeResponse represents the authenticated user's identity returned by the API: a minimal, immutable payload containing the user's Id and Email. Reach for it when you need to surface just enough identity information in responses (for example, a /me endpoint) while keeping the domain model private.

## Remarks
MeResponse serves as a stable, minimal surface for the authenticated identity. By using a record, it benefits from value-based equality and a concise, immutable shape that maps cleanly to JSON. It helps decouple the API contract from the domain model and provides a predictable payload for clients.
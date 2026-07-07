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


MeResponse is a small, immutable data carrier that represents a user's identity in authentication-related API surfaces. As a C# record, it provides value-based equality and concise construction, exposing two fields: Id (Guid) and Email (string). It is typically used as a minimal, serializable contract when returning information about the currently authenticated user, without pulling in the entire user profile.

## Remarks
MeResponse serves as a boundary between the authentication subsystem and API surfaces. It uses a record to guarantee value semantics and straightforward equality, and to enable compact construction with two identity fields. As a DTO, it is designed for serialization, exposing Id and Email for consumption by clients without exposing additional internal state.

## Notes
- Email is personally identifiable information; ensure proper handling, access control, and redaction in logs and telemetry.
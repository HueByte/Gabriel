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


MeResponse is a tiny, immutable payload used as the API contract for the authenticated user’s identity. It carries the user’s Id (Guid) and Email (string) as a simple, stable response object; as a positional-record, it benefits from value-based equality, concise construction, and deconstruction for convenient use in responses and tests.

## Remarks
MeResponse exists to decouple the external API surface from the internal user model. By exposing only Id and Email, it preserves a minimal yet useful identity contract for clients and enables evolution of the domain model without breaking API consumers. Its immutability and built-in equality help avoid accidental mutation and simplify comparisons in tests and pipelines.

## Notes
- ToString prints both Id and Email; be careful with verbose logs or telemetry. Redact Email if logging MeResponse in production.
- MeResponse is intended as a stable cross-boundary contract; add additional fields only in a separate response type to avoid breaking changes.
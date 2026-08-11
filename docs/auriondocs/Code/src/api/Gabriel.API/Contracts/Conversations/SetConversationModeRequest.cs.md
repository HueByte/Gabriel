# SetConversationModeRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/SetConversationModeRequest.cs`  
> **Kind:** record

```csharp
public sealed record SetConversationModeRequest(string? Mode)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Mode` | `string?` | — |


This record serves as the request body for updating a conversation's mode via the API. It carries a single optional Mode value; the value must be the lowercase name of one of the supported modes (chatty, elaborative, concise, tutor, critic) or null to reset back to the default (treated as chatty at read time). This payload is sent with a PUT to /api/conversations/{id}/mode to apply the requested mode.

## Remarks
Using a dedicated SetConversationModeRequest type provides a clear boundary for this API operation, decoupling the transport payload from any internal domain enum. The Mode property being a string allows for a forgiving boundary where null signals a reset, while the runtime can validate values against the known set. This design makes evolving the API easier: new modes or additional options can be added to the contract without changing existing signatures.

## Notes
- No validation is performed within this symbol; ensure Mode is either a valid lowercase name or null before sending.
- Null resets to default; ensure server semantics align with read-time default.
- The documentation comment in the source explains the accepted values; actual enforcement occurs at the API boundary.
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


SetConversationModeRequest is the API-facing payload used to specify the desired response style for a single conversation. It carries an optional Mode value, which must be the lowercased name of one of the predefined modes (chatty, elaborative, concise, tutor, critic); passing null clears any override and reverts to the default (treated as chatty at read time).

## Remarks
This is a lightweight, immutable payload that cleanly separates API surface from the domain logic. By allowing Mode to be null, it encodes a reset to the default style, while providing a clear, strongly-typed contract for setting a specific lowercased mode name when needed.

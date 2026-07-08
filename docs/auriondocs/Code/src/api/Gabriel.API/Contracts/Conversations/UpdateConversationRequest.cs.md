# UpdateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/UpdateConversationRequest.cs`  
> **Kind:** record

```csharp
public record UpdateConversationRequest(string Title)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Title` | `string` | — |


Represents the payload used to update a conversation's title. This C# record is an immutable, value-based carrier that encapsulates the new Title as its sole property. Use UpdateConversationRequest when calling the update-conversation API to express the intention to change a conversation's title, rather than passing a raw string or multiple fields.

## Remarks
Records in C# provide value-based equality and built-in deconstruction, making UpdateConversationRequest a predictable and easy-to-use data contract at API boundaries. Because it is defined as a positional record, Title is populated via the primary constructor and exposed as a read-only property, ensuring the payload remains consistent once created. The type is simple to extend with additional fields in the future if the API grows, while preserving a stable, forward-compatible shape for existing clients.

## Example
```csharp
var request = new UpdateConversationRequest("Project Kickoff");
```

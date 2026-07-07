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


Represents the payload sent to update a conversation's title in the Gabriel API. This immutable, single-field record wraps the new title value as a value object suitable for serialization in the request body. Create a new instance with the desired title and pass it to the update operation when renaming a conversation.

## Remarks
Because it's a record, it provides value-based equality and immutability, which makes it safe to reuse and compare as a request payload. It encapsulates the update of a conversation's title into a focused contract, decoupled from other conversation properties, which simplifies API surface and validation.

## Notes
- The Title parameter is required; the positional constructor enforces providing a non-null string for the new title.
- The record is immutable; to change the title, instantiate a new UpdateConversationRequest rather than mutating an existing instance.
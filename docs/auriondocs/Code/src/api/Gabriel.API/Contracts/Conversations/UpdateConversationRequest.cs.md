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


Represents the payload for updating a conversation's Title. This record is a simple, immutable data carrier used by the API to convey the new Title for a conversation. When performing an update, instantiate UpdateConversationRequest with the desired Title and pass it to the update operation, rather than constructing ad-hoc payloads.

## Remarks
Using a record provides immutability and value-based equality, making UpdateConversationRequest a safe carrier of the new Title across boundaries. This abstraction centralizes the update contract so future fields can be added without breaking existing call sites, and it pairs well with serialization layers on API boundaries.

## Example
```csharp
var request = new UpdateConversationRequest("New Title");
```

# CreateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/CreateConversationRequest.cs`  
> **Kind:** record

```csharp
public record CreateConversationRequest(string? Title, Guid? ProjectId)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Title` | `string?` | — |
| `ProjectId` | `Guid?` | — |


Represents the payload sent to create a new conversation via the Gabriel API. It exposes two optional fields: Title and ProjectId. Use this when issuing a create-conversation request; provide a Title to set a human-friendly label, and optionally assign the conversation to a specific project via ProjectId. If ProjectId is omitted, the backend will place the new conversation in the user's Default project (auto-created if needed). Because it is a C# record, it benefits from value-based equality and concise initialization, and it is ideal for transport across API boundaries.

## Remarks
This abstraction cleanly captures the contract of the API for creation, decoupling client code from backend persistence decisions. The optional fields reflect the fact that callers may prefer defaults or minimal payloads. As a record, it supports immutable semantics and easy equality checks, which help in testing and change detection, while keeping the payload lightweight for transport.

## Example
```csharp
// Title only
var req1 = new CreateConversationRequest(Title: "Design Review", ProjectId: null);

// ProjectId only
var req2 = new CreateConversationRequest(Title: null, ProjectId: Guid.Parse("11111111-1111-1111-1111-111111111111"));

// Both title and project
var req3 = new CreateConversationRequest(Title: "Sprint Planning", ProjectId: Guid.Parse("22222222-2222-2222-2222-222222222222"));
```

## Notes
- Nullability: Both Title and ProjectId are nullable, allowing callers to omit either field as needed.
- Immutability: Being a record, CreateConversationRequest is immutable; use a `with` expression to derive variations if needed.

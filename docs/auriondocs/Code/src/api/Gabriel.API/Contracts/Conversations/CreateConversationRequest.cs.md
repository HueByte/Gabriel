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


CreateConversationRequest is a small, immutable data transfer object used to request the creation of a new conversation. It carries an optional Title and an optional ProjectId; if no ProjectId is provided, the operation targets the user's Default project.

## Remarks
This record serves as a single transport for the create-conversation operation, ensuring both the optional title and optional project association are provided together. It prevents scattering raw values across the API boundary and makes the intent explicit—creating a conversation with an optional title and possible project scoping. The nullable nature of both fields documents the default/implicit behaviors: omit the title to let the server assign or omit the project to use the default project.

## Example
```csharp
// Create with a title and no project specified (use Default project)
var requestA = new CreateConversationRequest("Sprint Planning", null);

// Create with a specific project and no title
var requestB = new CreateConversationRequest(null, Guid.Parse("12345678-1234-1234-1234-1234567890ab"));
```

## Notes
- Title is optional; passing null is allowed and indicates no client-provided title.
- If ProjectId is null, the new conversation will be placed in the Default project (auto-created if necessary).
- As a record, this type provides value-based equality and immutability, making it well-suited for transport across API boundaries.
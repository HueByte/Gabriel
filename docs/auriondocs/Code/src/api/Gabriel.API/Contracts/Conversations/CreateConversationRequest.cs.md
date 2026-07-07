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


Represents the payload sent to create a new conversation. It carries an optional Title and an optional ProjectId. If ProjectId is not supplied, the new conversation will be created in the user's Default project (which will be auto-created if it doesn't exist yet).

## Remarks
This symbol serves as a minimal data container for the CreateConversation operation. Declaring it as a record ensures value-based equality and immutability, which makes it simple to compare requests and reason about them as a unit. The optional ProjectId enables explicit routing to a particular project; omitting it delegates the routing to the server by using the user's Default project, which may be auto-created if absent.

## Example
```csharp
// Create a conversation in the default project with a title
var requestDefault = new CreateConversationRequest("Team Sync", null);

// Create a conversation in a specific project
Guid someProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var requestWithProject = new CreateConversationRequest("Project Kickoff", someProjectId);
```

## Notes
- Leaving Title null may yield a conversation with no visible title; provide a meaningful title when possible to improve UX, unless the API explicitly allows unnamed conversations.
- The Title field is optional; depending on server-side rules, a non-empty title might still be required for persistence or display.
- Records provide value-based equality; two instances with identical Title and ProjectId are considered equal, which aids in testing and deduplication scenarios.
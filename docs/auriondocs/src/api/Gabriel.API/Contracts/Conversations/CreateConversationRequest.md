# CreateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/CreateConversationRequest.cs`  
> **Kind:** record

```csharp
// `ProjectId` is optional - if absent, the conversation lands in the user's
// Default project (auto-created if it doesn't exist yet).
public record CreateConversationRequest(string? Title, Guid? ProjectId)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `yet` | `auto-created if it doesn't exist` | — |


Represents the request payload for creating a new conversation via the API. Use this DTO when calling the conversations create endpoint to supply an optional title and an optional project identifier; omitting ProjectId places the conversation into the user's Default project (which will be auto-created if necessary).

## Remarks
This is a small immutable record used as an API contract/DTO. It captures only the minimal data the server needs to create a conversation: an optional human-readable Title and an optional ProjectId to associate the conversation with an existing project. If ProjectId is null the server will associate the conversation with the caller's Default project (auto-creating that project if it doesn't already exist).

## Example
```csharp
// Create a conversation with just a title (uses the Default project)
var req1 = new CreateConversationRequest("Brainstorming ideas", null);

// Create a conversation and associate it with an existing project
var projectId = Guid.Parse("d3b07384-d9a6-4f1a-9f87-1a2b3c4d5e6f");
var req2 = new CreateConversationRequest("Weekly sync notes", projectId);

// Title can be null if no initial name is desired
var req3 = new CreateConversationRequest(null, null);
```

## Notes
- Title is nullable; do not assume the server will set or return a non-empty title.  
- Passing null for ProjectId triggers placement in the user's Default project and may cause that project to be auto-created.  
- The record is immutable; use the with-expression to derive modified instances (e.g., `req2 with { Title = "New" }`).
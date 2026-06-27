# CreateConversationRequest

> **File:** `src/api/Gabriel.API/Contracts/Conversations/CreateConversationRequest.cs`  
> **Kind:** record

Represents the payload used when creating a new conversation via the API. Use this simple DTO when calling the Conversations create endpoint to supply an optional human-readable title and an optional project identifier; omit ProjectId to place the conversation in the user's Default project.

## Remarks
This record is an immutable, minimal API contract intended for transport only — it does not contain behavior or validation logic. The server-side implementation interprets a missing ProjectId as an instruction to associate the conversation with the caller's Default project (the Default project will be created if it does not already exist). Provide a ProjectId when you need the conversation created inside a specific existing project.

## Example
```csharp
// Create a conversation with a title and let the server pick the default project
var req1 = new CreateConversationRequest("Ideas for Q3", projectId: null);

// Create a conversation inside a specific project
var req2 = new CreateConversationRequest("Design discussion", projectId: Guid.Parse("d2f3c1a3-4b5e-4f6a-9c8e-0123456789ab"));

// Create a conversation with no title
var req3 = new CreateConversationRequest(title: null, projectId: null);
```

## Notes
- ProjectId is nullable: when null the conversation will be placed in the user's Default project (auto-created if necessary).
- Title is nullable; if omitted or empty the server may assign a default title.
- This type is a plain DTO — any domain validation or persistence behavior happens on the server side.
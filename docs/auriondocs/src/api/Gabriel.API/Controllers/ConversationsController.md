# ConversationsController

> **File:** `src/api/Gabriel.API/Controllers/ConversationsController.cs`  
> **Kind:** class

Exposes HTTP endpoints for managing user conversations: listing, creating, retrieving, renaming, and performing avatar/skin operations. The controller is the API surface used by authenticated clients to interact with conversation state and delegates business work to IChatService, IAgentService, IGabrielSequenceService and IProjectService.

## Remarks
This controller is a thin HTTP layer that forwards requests to underlying services and shapes responses as ConversationResponse objects. It intentionally avoids loading full project metadata for list results (to prevent an N+1 query pattern) but will enrich single-conversation responses with project information via the LoadProjectAsync helper. Server-sent-event / sequence streaming functionality and agent-related endpoints are implemented through the injected Gabriel/agent services so the controller coordinates streaming, JSON options, and cancellation tokens rather than implementing model logic.

## Example
```csharp
// List conversations (authenticated HttpClient)
var resp = await httpClient.GetAsync("/conversations");
resp.EnsureSuccessStatusCode();
var listJson = await resp.Content.ReadAsStringAsync();
// Parse into IReadOnlyList<ConversationResponse> using your serializer

// Get a single conversation by id
var convResp = await httpClient.GetAsync($"/conversations/{conversationId}");
convResp.EnsureSuccessStatusCode();
var convJson = await convResp.Content.ReadAsStringAsync();
```

## Notes
- All endpoints require authentication (controller marked with [Authorize]).
- The List endpoint accepts an optional projectId query parameter; omitting it returns "all my conversations" and intentionally excludes messages to avoid extra work.
- Conversation-level avatar skins can be pinned (PUT /{id}/skin) but pinned skins are ignored at render time for conversations that belong to a real project (the project's skin takes precedence); the controller persists the setting for potential future use.
- Most endpoints accept a CancellationToken and rely on the injected services to honor cancellation and validation.
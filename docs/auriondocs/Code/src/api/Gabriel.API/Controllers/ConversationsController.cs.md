# ConversationsController

> **File:** `src/api/Gabriel.API/Controllers/ConversationsController.cs`  
> **Kind:** class

A controller that exposes authenticated HTTP endpoints for managing user conversations: listing, retrieving, creating, renaming and performing conversation-level avatar/skin operations. Reach for this controller when implementing client features that need to display conversation lists, open a specific conversation (optionally including messages), or modify conversation metadata (title, avatar, skin).

## Remarks
This API surface is a thin orchestration layer that delegates core work to injected services (IChatService, IAgentService, IGabrielSequenceService, IProjectService) and converts domain objects to ConversationResponse DTOs. A small helper (LoadProjectAsync) enriches single-conversation responses with project context (used to determine project defaults and effective avatar seeds); that enrichment is intentionally skipped by the List endpoint to avoid N+1 queries and because sidebar rows do not render avatars.

The controller is decorated with [ApiController] and [Authorize], so all endpoints require an authenticated principal. A static JsonSerializerOptions (SseJsonOpts) is prepared with web-friendly defaults for any server-sent-events responses elsewhere in the controller.

## Example
```csharp
// GET /conversations (list)
using var client = new HttpClient { BaseAddress = new Uri("https://api.example.com/") };
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "<token>");
var resp = await client.GetAsync("conversations");
resp.EnsureSuccessStatusCode();
var convs = JsonSerializer.Deserialize<List<ConversationResponse>>(await resp.Content.ReadAsStringAsync());

// POST /conversations (create)
var createReq = new CreateConversationRequest { ProjectId = null, Title = "New chat" };
var content = new StringContent(JsonSerializer.Serialize(createReq), Encoding.UTF8, "application/json");
var createResp = await client.PostAsync("conversations", content);
createResp.EnsureSuccessStatusCode();
var created = JsonSerializer.Deserialize<ConversationResponse>(await createResp.Content.ReadAsStringAsync());
```

## Notes
- List returns ConversationResponse items with includeMessages: false and purposely does not load project data to avoid N+1 database calls; individual Get/Create/Update responses include project enrichment via LoadProjectAsync.
- PUT /{id}/skin uses PUT semantics and validates against the project's catalog (matching project behavior); pinned conversation skins apply only to standalone (default-project) conversations and are ignored at render time for real-project chats (though still persisted).
- All endpoints require authentication; clients must supply a valid bearer token or other configured auth mechanism.
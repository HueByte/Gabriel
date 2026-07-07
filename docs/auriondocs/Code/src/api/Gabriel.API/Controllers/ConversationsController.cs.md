# ConversationsController

> **File:** `src/api/Gabriel.API/Controllers/ConversationsController.cs`  
> **Kind:** class

*Figure: How ConversationsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ConversationsController["ConversationsController ctor injects IChatService IAgentService IGabrielSequenceService IProjectService PersonalityOptions"]
List["List endpoint: call IChatService.ListConversationsAsync(projectId)"]
Get["Get endpoint: call IChatService.GetConversationAsync(id)"]
Create["Create endpoint: receive CreateConversationRequest"]
Update["Update endpoint: receive UpdateConversationRequest"]
IChatService[IChatService]
IProjectService[IProjectService]
ConversationResponse[ConversationResponse]
CreateConversationRequest[CreateConversationRequest]
UpdateConversationRequest[UpdateConversationRequest]
Project[Project]

ConversationsController --> List
List --> IChatService
IChatService --> ConversationResponse
ConversationResponse --> ConversationsController

ConversationsController --> Get
Get --> IChatService
IChatService --> IProjectService
IProjectService --> ConversationResponse
ConversationResponse --> ConversationsController

ConversationsController --> Create
Create --> CreateConversationRequest
CreateConversationRequest --> IChatService
IChatService --> IProjectService
IProjectService --> ConversationResponse
ConversationResponse --> ConversationsController

ConversationsController --> Update
Update --> UpdateConversationRequest
UpdateConversationRequest --> IChatService
IChatService --> IProjectService
IProjectService --> ConversationResponse
ConversationResponse --> ConversationsController
```

```csharp
[ApiController]
[Authorize]
[Route("conversations")]
public class ConversationsController : ControllerBase
```


HTTP API surface for managing user conversations. Use this controller when you need REST endpoints to list, read, create and modify conversations (including avatar and skin operations); it sits behind authorization and exposes conversation payloads as ConversationResponse objects.

## Remarks
ConversationsController is the web API layer that translates HTTP requests into calls against the chat/project/agent/sequence services. It centralizes conversation-related endpoints (listing, retrieval, creation, renaming, avatar operations and related flows), enriches conversation responses with project data when appropriate, and uses the configured PersonalityOptions to influence behavior where needed. The controller intentionally avoids per-row project loading on the list endpoint to prevent N+1 queries (single-conversation endpoints do load the parent project so responses can include projectIsDefault and avatar seed information).

## Notes
- The List endpoint accepts an optional projectId query parameter; omitting it returns "all my conversations" while providing it scopes results to a single project.
- The controller is decorated with [Authorize], so all routes require an authenticated user, and most actions accept a CancellationToken to allow request cancellation.
- PUT /{id}/skin uses PUT semantics and validates skin against the project catalog; note that a pinned conversation skin is ignored at render time for conversations that belong to a real project (the value is still persisted).
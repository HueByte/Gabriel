# ConversationsController

> **File:** `src/api/Gabriel.API/Controllers/ConversationsController.cs`  
> **Kind:** class

*Figure: How ConversationsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
Start["ConversationsController receives HTTP request"] --> Route{"Which endpoint?"}

Route -->|"List"| ListCall["Call IChatService.ListConversationsAsync(projectId)"]
ListCall --> ListMap["Map each to ConversationResponse (includeMessages false)"]
ListMap --> ListResp["Return 200 OK with List<ConversationResponse>"]

Route -->|"Get {id}"| GetCall["Call IChatService.GetConversationAsync(id)"]
GetCall --> CheckProjGet{"conv.ProjectId present?"}
CheckProjGet -->|"Yes"| GetProject["Call IProjectService.GetAsync(pid) -> Project"]
CheckProjGet -->|"No"| NoProjectGet["Project = null"]
GetProject --> GetMap["Map to ConversationResponse (includeMessages true, project)"]
NoProjectGet --> GetMap
GetMap --> GetResp["Return 200 OK with ConversationResponse"]

Route -->|"Create"| CreateReq["Receive CreateConversationRequest"]
CreateReq --> CreateCall["Call IChatService.CreateConversationAsync(request.ProjectId, request.Title)"]
CreateCall --> CheckProjCreate{"conv.ProjectId present?"}
CheckProjCreate -->|"Yes"| GetProjectCreate["Call IProjectService.GetAsync(pid) -> Project"]
CheckProjCreate -->|"No"| NoProjectCreate["Project = null"]
GetProjectCreate --> CreateMap["Create ConversationResponse (includeMessages true, project)"]
NoProjectCreate --> CreateMap
CreateMap --> CreatedResp["Return 201 CreatedAtAction(Get) with ConversationResponse"]

Route -->|"Update {id}"| UpdateReq["Receive UpdateConversationRequest"]
UpdateReq --> UpdateCall["Call IChatService.RenameConversationAsync(id, request.Title)"]
UpdateCall --> CheckProjUpdate{"conv.ProjectId present?"}
CheckProjUpdate -->|"Yes"| GetProjectUpdate["Call IProjectService.GetAsync(pid) -> Project"]
CheckProjUpdate -->|"No"| NoProjectUpdate["Project = null"]
GetProjectUpdate --> UpdateMap["Map to ConversationResponse (includeMessages false, project)"]
NoProjectUpdate --> UpdateMap
UpdateMap --> UpdateResp["Return 200 OK with ConversationResponse"]
```

```csharp
[ApiController]
[Authorize]
[Route("conversations")]
public class ConversationsController : ControllerBase
```


API controller that exposes REST endpoints for managing user conversations and performing conversation-related operations (list, get, create, rename, avatar/skin operations and related sequence/agent interactions) under the /conversations route. Use this controller when a client needs to enumerate a user's conversations, read a single conversation (including messages), create or rename conversations, or invoke conversation-specific behaviors like avatar rerolls and skin management. All endpoints require an authenticated user.

## Remarks
This controller is a thin HTTP layer that delegates business logic to application services (IChatService, IAgentService, IGabrielSequenceService, IProjectService) and shapes the results into ConversationResponse DTOs. Single-conversation endpoints load the conversation's parent project to surface project-specific metadata (for example projectIsDefault and effectiveAvatarSeed); the List endpoint deliberately avoids that extra load and omits messages for performance and to prevent an N+1 query pattern. The controller also configures JSON options used for server-sent-event/sequence responses.

## Notes
- All routes are protected by [Authorize]; callers must be authenticated.
- The List endpoint returns conversation summaries with includeMessages: false to avoid loading full message histories and to prevent N+1 project lookups; single-item endpoints include project data and (where appropriate) message lists.
- Pinning a conversation skin (SetSkin) is meaningful only for standalone/default-project conversations; project-backed conversations render the project's skin and will ignore a pinned conversation skin at render time (the pinned value is still persisted).
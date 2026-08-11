# ConversationsController

> **File:** `src/api/Gabriel.API/Controllers/ConversationsController.cs`  
> **Kind:** class

*Figure: How ConversationsController works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
ctrl["ConversationsController receives HTTP request"]

ctrl --> list_ep["List: GET /conversations"]
ctrl --> get_ep["Get: GET /conversations/{id}"]
ctrl --> create_ep["Create: POST /conversations (body: CreateConversationRequest)"]
ctrl --> update_ep["Update: PATCH /conversations/{id} (body: UpdateConversationRequest)"]

list_ep --> chat_list["IChatService.ListConversationsAsync(projectId)"]
chat_list --> map_list["Map each to ConversationResponse (includeMessages: false)"]
map_list --> ok_list["Return 200 Ok(IReadOnlyList<ConversationResponse>)"]

get_ep --> chat_get["IChatService.GetConversationAsync(id)"]
chat_get --> loadproj_get["LoadProjectAsync(conv.ProjectId)"]
loadproj_get --> proj_null_get{"projectId is null?"}
proj_null_get -->|"Yes"| proj_none_get["Project = null"]
proj_null_get -->|"No"| proj_fetch_get["IProjectService.GetAsync(pid)"]
proj_fetch_get --> proj_loaded_get["Project returned"]
proj_none_get --> map_get["conv.ToResponse(includeMessages: true, project)"]
proj_loaded_get --> map_get
map_get --> ok_get["Return 200 Ok(ConversationResponse)"]

create_ep --> chat_create["IChatService.CreateConversationAsync(projectId, title)"]
chat_create --> loadproj_create["LoadProjectAsync(conv.ProjectId)"]
loadproj_create --> proj_null_create{"projectId is null?"}
proj_null_create -->|"Yes"| proj_none_create["Project = null"]
proj_null_create -->|"No"| proj_fetch_create["IProjectService.GetAsync(pid)"]
proj_fetch_create --> proj_loaded_create["Project returned"]
proj_none_create --> map_create["conv.ToResponse(includeMessages: true, project)"]
proj_loaded_create --> map_create
map_create --> created["Return 201 CreatedAtAction(Get, { id })"]

update_ep --> chat_update["IChatService.RenameConversationAsync(id, title)"]
chat_update --> loadproj_update["LoadProjectAsync(conv.ProjectId)"]
loadproj_update --> proj_null_update{"projectId is null?"}
proj_null_update -->|"Yes"| proj_none_update["Project = null"]
proj_null_update -->|"No"| proj_fetch_update["IProjectService.GetAsync(pid)"]
proj_fetch_update --> proj_loaded_update["Project returned"]
proj_none_update --> map_update["conv.ToResponse(includeMessages: false, project)"]
proj_loaded_update --> map_update
map_update --> ok_update["Return 200 Ok(ConversationResponse)"]
```

```csharp
[ApiController]
[Authorize]
[Route("conversations")]
public class ConversationsController : ControllerBase
```


Exposes the HTTP endpoints for managing conversation resources under the "conversations" route. This ApiController is protected with [Authorize] and delegates conversation operations (list, get, create, rename, avatar reroll, skin pinning) to backend services such as IChatService, IAgentService and IProjectService; use it when you need a REST surface for CRUD and conversation-specific actions rather than calling service-layer APIs directly.

## Remarks
The controller intentionally keeps thin HTTP actions and forwards work to injected services. A private helper, LoadProjectAsync, is used by single-conversation endpoints to load the conversation's parent project so the response can include project-specific metadata (for example, whether the project is the default and the effective avatar seed). The List endpoint intentionally skips loading project data to avoid an N+1 query pattern and because sidebar list rows do not render avatars.

## Notes
- The List endpoint returns ConversationResponse objects with messages omitted (the controller calls ToResponse(includeMessages: false)). Single-conversation endpoints (Get, Create) return responses that include messages.
- The optional projectId query parameter on List scopes the result to a project; omitting it returns all conversations for the caller.
- PUT /{id}/skin is intended to pin/clear a conversation avatar skin; this is meaningful for standalone (default-project) conversations. For real project-backed conversations the project's skin is rendered instead (a pinned conversation skin is persisted but ignored at render time).
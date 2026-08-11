# ChatService

> **File:** `src/api/Gabriel.Core/Services/ChatService.cs`  
> **Kind:** class

*Figure: How ChatService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
start["Start CreateConversationAsync"] --> req["Call RequireUserId() via ICurrentUser"]
req --> checkIsPid{"Is projectId provided?"}
checkIsPid -- Yes --> getProject["IProjectRepository.GetByIdAsync(pid, userId)"]
getProject --> projectFound{"Project found?"}
projectFound -- No --> notFound["Throw NotFoundException(nameof(Project), pid)"]
notFound --> endErr["End (NotFoundException)"]
projectFound -- Yes --> setResolvedFromProject["resolvedProjectId = Project.Id"]
checkIsPid -- No --> ensureDefault["IProjectService.EnsureDefaultProjectIdAsync()"]
ensureDefault --> setResolvedDefault["resolvedProjectId = returned default id"]
setResolvedFromProject --> create["Conversation.Create(userId, resolvedProjectId, title)"]
setResolvedDefault --> create
create --> add["IConversationRepository.AddAsync(conversation)"]
add --> save["IUnitOfWork.SaveChangesAsync()"]
save --> ret["Return Conversation"]
ret --> end["End"]
```

```csharp
public class ChatService : IChatService
```


Provides application-level operations for managing Conversation entities on behalf of the current authenticated user. Use this service when you need to create, enumerate, retrieve or modify conversations while enforcing ownership, resolving project placement (including the user's default project), and committing changes via the unit-of-work.

## Remarks
ChatService coordinates repository and domain operations rather than containing domain logic itself: it resolves the target project (verifying supplied project IDs belong to the current user or falling back to the user's default project), fetches or validates Conversation instances from IConversationRepository, calls domain methods on Conversation (Rename, RerollAvatar, SetSkin, SetMode, etc.), and persists changes through IUnitOfWork. It centralizes common checks (current user resolution, NotFound handling) so callers do not need to repeat ownership or persistence concerns.

## Example
```csharp
// Typical usage from an API controller or application layer that has an IChatService:
var ct = CancellationToken.None;

// Create a conversation in the caller's default project
var created = await chatService.CreateConversationAsync(projectId: null, title: "Ideas", ct);

// Rename the conversation
var renamed = await chatService.RenameConversationAsync(created.Id, "New Ideas", ct);

// Retrieve the conversation with messages
var loaded = await chatService.GetConversationAsync(renamed.Id, ct);

// List conversations (optionally scoped to a project)
var list = await chatService.ListConversationsAsync(projectId: null, ct);
```

## Notes
- If a caller supplies a projectId, the service verifies that the project belongs to the current user; otherwise it throws NotFoundException for that project id.
- Domain-level validation (for example, Conversation.Rename rejecting an empty or whitespace title) is performed by the Conversation entity; such validation surfaces as exceptions (ArgumentException) and is expected to be handled by the application's global exception mapping.
- All operations require an authenticated user (the service resolves the current user id); missing authentication will prevent operations from proceeding.
- Methods persist changes by calling SaveChangesAsync on the unit-of-work; callers should not attempt an additional save for the same operation.
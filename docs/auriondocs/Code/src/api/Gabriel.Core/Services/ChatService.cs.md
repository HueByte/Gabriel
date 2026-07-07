# ChatService

> **File:** `src/api/Gabriel.Core/Services/ChatService.cs`  
> **Kind:** class

*Figure: How ChatService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
start["CreateConversationAsync (ChatService)"]
requireUser["Call RequireUserId (ICurrentUser)"]
decision{"projectId supplied?"}
getProject["IProjectRepository.GetByIdAsync(pid, userId) (IProjectRepository)"]
projNotFound["Throw NotFoundException (NotFoundException)"]
useProject["resolvedProjectId = project.Id (Project)"]
ensureDefault["IProjectService.EnsureDefaultProjectIdAsync() (IProjectService)"]
createConv["Conversation.Create(userId, resolvedProjectId, title) (Conversation)"]
addConv["IConversationRepository.AddAsync(conversation) (IConversationRepository)"]
save["IUnitOfWork.SaveChangesAsync() (IUnitOfWork)"]
returnNode["Return Conversation"]

start --> requireUser
requireUser --> decision

decision --"yes"--> getProject
getProject --"found"--> useProject
getProject --"null"--> projNotFound

decision --"no"--> ensureDefault

useProject --> createConv
ensureDefault --> createConv
createConv --> addConv
addConv --> save
save --> returnNode
```

```csharp
public class ChatService : IChatService
```


Coordinates conversation-related operations and enforces per-user scope.

Use ChatService when you need the higher-level business behavior around conversations (create, list, rename, set skin/mode, delete messages, etc.) rather than manipulating repositories directly: it resolves the caller's user context, ensures project ownership or default project creation, invokes Conversation entity methods for invariants, and persists changes through the unit-of-work.

## Remarks
ChatService is an application-layer facade that composes IConversationRepository, IProjectService, IProjectRepository, IUnitOfWork and ICurrentUser to implement IChatService. It centralizes authorization (operations are executed for the current user obtained via RequireUserId), project resolution (validates a supplied project belongs to the user or falls back to the user's default project via EnsureDefaultProjectIdAsync) and persistence (calls SaveChangesAsync on the unit-of-work after repository updates). Domain invariants and validation are enforced by the Conversation entity (for example Rename, SetSkin, RerollAvatar), and missing resources are surfaced as NotFoundException so the API layer can translate them to the appropriate HTTP responses.

## Notes
- If the caller supplies a projectId to CreateConversationAsync the service verifies the project belongs to the current user; if it does not exist or is not accessible a NotFoundException is thrown for that project id.
- Conversation.Rename throws on invalid (empty/whitespace) titles; the service does not catch that and relies on the global exception handling to map it to a 400 Bad Request.
- Mutating operations call IUnitOfWork.SaveChangesAsync; changes are not persisted until that call completes.
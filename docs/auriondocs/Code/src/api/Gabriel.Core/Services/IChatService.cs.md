# IChatService

> **File:** `src/api/Gabriel.Core/Services/IChatService.cs`  
> **Kind:** interface

*Figure: How IChatService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
start["IChatService: receive API request (Create/List/Get/Modify/Delete)"]
start --> op{ "Operation type?" }

op -- "CreateConversationAsync" --> create["CreateConversationAsync: if projectId is null -> ensure Default project exists via IProjectService, then create Conversation"]
create --> c_return["Return Conversation"]

op -- "ListConversationsAsync" --> list["ListConversationsAsync: if projectId is null -> list across all projects; else filter by project"]
list --> list_return["Return IReadOnlyList<Conversation>"]

op -- "GetConversationAsync" --> get["GetConversationAsync: fetch Conversation by id"]
get --> g_return["Return Conversation"]

op -- "RenameConversationAsync" --> rename["RenameConversationAsync: update Conversation.title"]
rename --> r_return["Return Conversation"]

op -- "RerollAvatarAsync" --> reroll["RerollAvatarAsync: regenerate Conversation avatar"]
reroll --> rv_return["Return Conversation"]

op -- "SetSkinAsync" --> skin["SetSkinAsync: if Conversation in real project -> no-op (project skin applies); else set Conversation skin (catalog ids validated at API layer); mirror of IProjectService.SetSkinAsync"]
skin --> s_return["Return Conversation"]

op -- "SetModeAsync" --> mode["SetModeAsync: set GabrielMode on Conversation; null clears to default (treated as Chatty at read time)"]
mode --> m_return["Return Conversation"]

op -- "DeleteConversationAsync" --> delc["DeleteConversationAsync: delete Conversation"]
delc --> done["Done"]

op -- "DeleteMessageAsync" --> delm["DeleteMessageAsync: remove Message and all subsequent messages anchored on variant group's earliest sibling (rewind)"]
delm --> dm_return["Return Conversation"]

op -- "SetActiveVariantAsync" --> variant["SetActiveVariantAsync: mark chosen Message variant active; deactivate siblings; no-op if already active"]
variant --> v_return["Return Conversation"]

op -- "Other" --> agent["IAgentService: handles chat-turn loop (streaming/tool-calls) — outside lifecycle here"]
```

```csharp
public interface IChatService
```


A high-level CRUD orchestrator for conversations and per-message operations. Use this interface when you need to manage conversation lifecycle (create, list, rename, delete), control presentation attributes (avatar, skin), adjust conversation behaviour (mode), or perform structural message operations (delete a tail of messages or flip the active variant). The actual chat-turn loop, streaming and tool-invocation logic lives elsewhere (IAgentService); IChatService focuses on persistent conversation state and mutations.

## Remarks
IChatService is intentionally narrow: it does not implement the live agent/turn loop but instead centralises persistence and lifecycle concerns for conversations so higher-level services (for example the agent runtime and API controllers) can call into a single place to modify conversation state. Conversation-level skin settings are supported for standalone (Default) conversations but are superseded by a project's skin when the conversation belongs to a non-default project; callers should validate catalog identifiers for SetSkinAsync before invoking this interface. Methods return the updated Conversation where useful so callers can render the new state immediately.

## Example
```csharp
// Typical usage from an API controller or application layer
var convo = await chatService.CreateConversationAsync(projectId: null, title: "Morning ideas", ct: ct);
convo = await chatService.RenameConversationAsync(convo.Id, "Morning brainstorm", ct);
convo = await chatService.SetModeAsync(convo.Id, GabrielMode.Investigative, ct: ct);
convo = await chatService.RerollAvatarAsync(convo.Id, ct);
var all = await chatService.ListConversationsAsync(projectId: null, ct: ct);
```

## Notes
- DeleteMessageAsync is destructive: it removes the target message and everything after it (anchored to the variant group's earliest sibling) — used to implement "rewind to this point" UX.
- SetSkinAsync only affects conversations in the Default (standalone) project; conversations owned by a real project render the project's skin instead.
- Passing null to SetModeAsync clears any per-conversation override and restores the system default behaviour (read-time default is treated as Chatty).
- SetActiveVariantAsync flips the active flag within a message's variant group; it is a no-op if the specified message is already the active variant.
- All operations accept a CancellationToken and are asynchronous; callers should handle typical async/IO exceptions and cancellation.

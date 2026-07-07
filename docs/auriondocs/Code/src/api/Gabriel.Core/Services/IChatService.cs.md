# IChatService

> **File:** `src/api/Gabriel.Core/Services/IChatService.cs`  
> **Kind:** interface

*Figure: How IChatService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB
Request["Incoming request to IChatService"]
IChatService[IChatService]
IProjectService[IProjectService]
IAgentService[IAgentService]
Conversation[Conversation]
Message[Message]
GabrielMode[GabrielMode]

Request --> IChatService
IChatService --> choose["Switch by operation"]

choose --> create["CreateConversationAsync\n(projectId optional)"]
create --> checkProj["projectId is null?"]
checkProj -->|"yes"| ensureDefault["IProjectService: ensure Default project exists (create if absent)"]
ensureDefault --> createConv["Create Conversation record (Conversation)"]
checkProj -->|"no"| createConv
createConv --> returnC1["Return Conversation"]

choose --> list["ListConversationsAsync\n(optional project filter)"]
list --> returnList["Return list of Conversation(s)"]

choose --> get["GetConversationAsync / Rename / RerollAvatar / DeleteConversation"]
get --> returnC2["Return Conversation or void"]

choose --> setSkin["SetSkinAsync (pattern + palette)"]
setSkin --> skinCheck["Is Conversation in a real project?"]
skinCheck -->|"yes"| projectSkin["Project skin applies — conversation skin ignored (no-op)"]
skinCheck -->|"no"| applySkin["Set Conversation skin (pattern + palette)"]
applySkin --> returnC3["Return Conversation"]

choose --> setMode["SetModeAsync"]
setMode --> modeCheck["GabrielMode null?"]
modeCheck -->|"yes"| clearMode["Clear to default (treated as Chatty at read time)"]
modeCheck -->|"no"| applyMode["Set Conversation.GabrielMode to provided value"]
clearMode --> returnC4["Return Conversation"]
applyMode --> returnC4

choose --> deleteMsg["DeleteMessageAsync"]
deleteMsg --> anchor["Find variant group's earliest sibling (anchor)"]
anchor --> removeTail["Delete targeted Message and everything after anchor"]
removeTail --> returnC5["Return Conversation"]

choose --> setActive["SetActiveVariantAsync"]
setActive --> activeCheck["Is chosen Message already active?"]
activeCheck -->|"yes"| noop["No-op"]
activeCheck -->|"no"| flip["Mark chosen Message active; mark siblings inactive"]
flip --> returnC6["Return Conversation"]
noop --> returnC6

IChatService -.-> IAgentService["IAgentService\n(chat-turn loop & streaming handled here)"]
```

```csharp
public interface IChatService
```


A high-level conversation lifecycle API: create, list, retrieve, rename and delete conversations, manage per-conversation presentation (avatar/skin) and behaviour (mode), and perform message-level edits such as deleting a message tail or selecting an active variant. Use this when you need to manage conversation metadata and structural changes outside of the agent/chat-turn execution (the live streaming/tooling loop lives in IAgentService).

## Remarks
IChatService is an orchestration surface for conversation CRUD and structural edits — it does not implement the chat turn loop or runtime agent behavior. It coordinates per-conversation settings (title, avatar/skin overrides, mode) and structural operations on messages (rewinds and variant selection). Skin operations mirror IProjectService.SetSkinAsync: for conversations belonging to a real project the project's skin is authoritative; pattern/palette overrides on a conversation are only meaningful for standalone/default-project conversations. Project creation/lookup (the "default" project auto-created on first use) is managed at a higher layer (IProjectService).

## Example
```csharp
// Create a conversation in the user's Default project (projectId=null uses/creates the default)
var conversation = await chatService.CreateConversationAsync(projectId: null, title: "Ideas", ct: cancellationToken);

// Set a conversation-level skin (only used when the conversation is not inside a real project)
conversation = await chatService.SetSkinAsync(conversation.Id, pattern: "stripes", palette: "ocean", ct: cancellationToken);

// Delete a message and everything that followed it in that thread (a rewind)
conversation = await chatService.DeleteMessageAsync(conversation.Id, messageIdToRewindTo, ct: cancellationToken);
```

## Notes
- DeleteMessageAsync is destructive: it removes the targeted message and everything that came after it (the tail), anchored to the variant group's earliest sibling so regenerated tails are removed cleanly.
- SetActiveVariantAsync flips which sibling variant is active within its variant group; it is a no-op if the requested variant is already active.
- Passing null to SetModeAsync clears any per-conversation override and returns the conversation to the default behaviour (treated as "Chatty" at read time).
- Catalog identifiers for SetSkinAsync must be validated before calling this service; the interface assumes validated identifiers and does not re-check catalog validity.
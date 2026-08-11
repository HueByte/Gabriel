# IChatService

> **File:** `src/api/Gabriel.Core/Services/IChatService.cs`  
> **Kind:** interface

*Figure: How IChatService works.*

```mermaid
%%{init: {'theme':'base','themeVariables':{'background':'#faf7ef','primaryColor':'#f0e2c2','primaryTextColor':'#1f2840','primaryBorderColor':'#8a7548','secondaryColor':'#d9efec','secondaryBorderColor':'#1d8a80','secondaryTextColor':'#1f2840','tertiaryColor':'#f2ebd8','tertiaryBorderColor':'#8a7548','tertiaryTextColor':'#1f2840','lineColor':'#1d8a80','titleColor':'#1f2840','fontSize':'14px','edgeLabelBackground':'#faf7ef','clusterBkg':'#f2ebd8','clusterBorder':'#8a7548','actorBkg':'#f0e2c2','actorBorder':'#8a7548','actorTextColor':'#1f2840','actorLineColor':'#8a7548','signalColor':'#1d8a80','signalTextColor':'#1f2840','activationBkgColor':'#d9efec','activationBorderColor':'#1d8a80','noteBkgColor':'#f2ebd8','noteBorderColor':'#8a7548','noteTextColor':'#1f2840','labelBoxBkgColor':'#f0e2c2','labelBoxBorderColor':'#8a7548','labelTextColor':'#1f2840','transitionColor':'#1d8a80','transitionLabelColor':'#1f2840','stateLabelColor':'#1f2840','altBackground':'#f2ebd8'}}}%%
flowchart TB

Start["IChatService: Receive request"] --> OpDec{ "Which operation?" }

OpDec -->|Create| Create["CreateConversationAsync(projectId?, title)"]
Create --> ProjectCheck{ "projectId provided?" }
ProjectCheck -->|Yes| UseProject["IProjectService: Use project"]
ProjectCheck -->|No| EnsureDefault["IProjectService: Ensure Default project (auto-create)"]
UseProject --> CreateDone["Conversation: Create and return"]
EnsureDefault --> CreateDone

OpDec -->|List| List["ListConversationsAsync(projectId?)"]
List --> ListDecision{ "projectId null?" }
ListDecision -->|Yes| ListAll["Return all Conversations across projects"]
ListDecision -->|No| ListProj["Return Conversations for project"]
ListAll --> EndList["IChatService: Return IReadOnlyList<Conversation>"]
ListProj --> EndList

OpDec -->|Get/Rename/Reroll| GetOps["GetConversationAsync / RenameConversationAsync / RerollAvatarAsync"]
GetOps --> GetDone["Conversation: Fetch or update and return"]

OpDec -->|SetSkin| SetSkin["SetSkinAsync(id, pattern?, palette?)"]
SetSkin --> SkinCheck{ "Conversation in Default project?" }
SkinCheck -->|Yes| ApplySkin["Apply skin to Conversation (catalog IDs must be validated by API)"]
SkinCheck -->|No| SkinNoop["No-op: project skin overrides (mirror of IProjectService.SetSkinAsync)"]
ApplySkin --> SkinDone["Conversation: Return"]
SkinNoop --> SkinDone

OpDec -->|SetMode| SetMode["SetModeAsync(id, GabrielMode?)"]
SetMode --> ModeCheck{ "mode null?" }
ModeCheck -->|Yes| ClearMode["Clear per-conversation mode (reads as Chatty)"]
ModeCheck -->|No| ApplyMode["Set GabrielMode on Conversation"]
ClearMode --> ModeDone["Conversation: Return"]
ApplyMode --> ModeDone

OpDec -->|DeleteConversation| DelConv["DeleteConversationAsync(id)"]
DelConv --> DelConvDone["Conversation: removed"]

OpDec -->|Message-level ops| MsgOps["Message-level operations"]
MsgOps --> DeleteMsg["DeleteMessageAsync(conversationId, messageId)"]
DeleteMsg --> DeleteMsgProc["Delete target message AND everything after it (anchor at variant group's earliest sibling)"]
DeleteMsgProc --> MsgReturn["Conversation: Return"]

MsgOps --> SetVariant["SetActiveVariantAsync(conversationId, messageId)"]
SetVariant --> VariantCheck{ "chosen message already active?" }
VariantCheck -->|Yes| VariantNoop["No-op"]
VariantCheck -->|No| VariantApply["Flip siblings inactive; chosen variant becomes active"]
VariantNoop --> MsgReturn
VariantApply --> MsgReturn

%% end
```

```csharp
public interface IChatService
```


An orchestration-focused abstraction for conversation lifecycle and per-message operations. Use IChatService when you need to create, list, rename, delete or otherwise manage conversations and their message-level state (delete/rewind, pick active variant, set avatar skin or per-conversation mode). The actual chat-turn execution (streaming turns, tool invocation, model interaction) is intentionally kept out of this interface and belongs to IAgentService — IChatService concerns itself with CRUD and business rules around conversation metadata and structural edits.

## Remarks
IChatService centralizes conversation lifecycle behavior so callers do not need to manipulate Conversation entities or project-level skin logic directly. It preserves higher-level rules: a null projectId places a conversation into the user's Default project (which is auto-created on first use), SetSkinAsync is a per-conversation override but is only meaningful for conversations that are not inside a real project (project-owned chats render the project's skin instead), and message deletions are destructive rewinds (they remove the targeted message and everything that came after it to keep variant-group tails consistent). Implementations are expected to validate catalog identifiers (pattern/palette) at the API boundary before they reach SetSkinAsync.

## Example
```csharp
// Typical usage from an application service or controller
IChatService chat = /* resolved from DI */;
CancellationToken ct = CancellationToken.None;

// Create a conversation in the user's default project (pass null)
Conversation convo = await chat.CreateConversationAsync(null, "New chat", ct);

// Rename and list
convo = await chat.RenameConversationAsync(convo.Id, "Rename: brainstorming", ct);
IReadOnlyList<Conversation> all = await chat.ListConversationsAsync(null, ct);

// Pin a skin for a standalone conversation (pattern/palette must already be validated)
convo = await chat.SetSkinAsync(convo.Id, "stripes", "ocean", ct);

// Rewind the conversation by deleting a message (destructive: removes message + tail)
convo = await chat.DeleteMessageAsync(convo.Id, someMessageId, ct);

// Switch which variant is active in a variant group
convo = await chat.SetActiveVariantAsync(convo.Id, variantMessageId, ct);
```

## Notes
- SetSkinAsync: callers must validate catalog identifiers (pattern/palette) before calling; for conversations that belong to a real project the project's skin takes precedence and the conversation-level skin will not be used for rendering.
- DeleteMessageAsync is destructive by design: it removes the targeted message and everything that followed it (anchored at the variant group's earliest sibling) — use for "rewind to here" UX only.
- SetModeAsync accepts null to clear a custom mode back to the default; the read path treats the default as Chatty.
- SetActiveVariantAsync is a no-op if the requested message is already the active variant.
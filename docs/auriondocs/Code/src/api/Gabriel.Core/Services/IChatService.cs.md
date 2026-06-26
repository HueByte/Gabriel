# IChatService

> **File:** `src/api/Gabriel.Core/Services/IChatService.cs`  
> **Kind:** interface

Provides CRUD and message-level orchestration for user conversations. Reach for IChatService when you need to create, list, rename, delete or otherwise manage conversation lifecycle and per-message operations; the streaming chat-turn loop and tool-invocation behavior belong to IAgentService and should not be implemented here.

## Remarks
IChatService isolates lifecycle and structural changes to conversations from the runtime agent logic. It is responsible for project-scoped conversation creation (projectId is optional and a Default project is auto-created), metadata updates (title, avatar/skin, mode) and destructive message operations (rewinds and variant selection). Higher-level components (API controllers, agent services) call these methods rather than manipulating storage directly so that consistency, validation and project-level rules are centralized.

## Example
```csharp
// Typical sequence: create a conversation, set behaviour, and list
var convo = await chatService.CreateConversationAsync(projectId: null, title: "Research thread", ct: ct);
convo = await chatService.SetModeAsync(convo.Id, GabrielMode.Helpful, ct);
var all = await chatService.ListConversationsAsync(projectId: null, ct);

// Rewind a thread by deleting a message and everything after it
await chatService.DeleteMessageAsync(convo.Id, messageToRewindId, ct);

// Switch active variant within a variant group
await chatService.SetActiveVariantAsync(convo.Id, chosenVariantMessageId, ct);
```

## Notes
- projectId is optional: when null the conversation is placed in the user's Default project, which is created automatically on first use.
- SetSkinAsync affects only standalone (Default-project) conversations; conversations that belong to a real project render the project's skin instead. Catalog identifiers (pattern/palette) must be validated at the API layer before calling SetSkinAsync.
- DeleteMessageAsync is destructive: it removes the targeted message and everything that follows it (the operation anchors on the variant group's earliest sibling so a regeneration tail is removed cleanly).
- Passing null to SetModeAsync clears the per-conversation override and falls back to the default (treated as Chatty at read time).
- SetActiveVariantAsync is a no-op if the specified message is already the active variant.
- Respect CancellationToken parameters; implementations are expected to surface cancellation promptly where applicable.

# IChatService

> **File:** `src/api/Gabriel.Core/Services/IChatService.cs`  
> **Kind:** interface

Provides high-level conversation lifecycle and message-level operations (create, list, get, rename, delete, avatar/skin changes, mode toggles, and message variant management). Reach for IChatService when you need to orchestrate CRUD and non-streaming changes to conversations; the actual chat-turn loop and streaming/tool-invocation behavior are handled separately by IAgentService.

## Remarks
This interface separates conversation lifecycle concerns from the agent's runtime loop: it manages storage, metadata, and structural edits (for example, rewinding a thread or switching active message variants) while leaving turn-by-turn streaming and tool execution to the agent layer. Several operations are scoped to project semantics — a null projectId targets the user's Default project (which is auto-created on first use) and skins set on conversations are meaningful only for standalone/default-project chats; projects with their own skin render the project's skin instead.

## Example
```csharp
// Create a conversation in the Default project, rename it, then set a conversational mode.
var convo = await chatService.CreateConversationAsync(projectId: null, title: "Brainstorming");
convo = await chatService.RenameConversationAsync(convo.Id, "Product Ideas");
convo = await chatService.SetModeAsync(convo.Id, GabrielMode.Inquisitive);

// Delete a message and everything that followed it (used for "rewind to here").
await chatService.DeleteMessageAsync(convo.Id, messageIdToRewindTo);

// Switch the active variant for a variant group inside the conversation.
convo = await chatService.SetActiveVariantAsync(convo.Id, chosenVariantMessageId);
```

## Notes
- Passing null as projectId uses the caller's Default project; the implementation may auto-create that project on first use.
- SetSkinAsync is only meaningful for conversations not bound to a real project; project-scoped conversations use the project's skin instead. Catalog pattern/palette identifiers should be validated before calling SetSkinAsync.
- DeleteMessageAsync is destructive: it removes the targeted message and all subsequent content anchored to the variant group's earliest sibling — intended for "rewind this thread to here" UX.
- SetActiveVariantAsync flips the active flag among sibling variants; it is a no-op if the requested message is already the active variant.
- Passing null to SetModeAsync clears any conversation-specific mode back to the default (treated as Chatty at read time).

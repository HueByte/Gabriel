# Conversation

> **File:** `src/api/Gabriel.Core/Entities/Conversation.cs`  
> **Kind:** class

Represents a persisted, user-scoped chat thread and its metadata. Use this entity when creating, loading, or persisting conversations through the repository layer; instantiate new conversations via Conversation.Create(...) rather than the EF-only parameterless constructor.

## Remarks
Conversation models the conversation-level state required by the application and by downstream services: ownership (UserId) is enforced so repository queries present only a user's threads; optional ProjectId supports project containment while remaining nullable for backwards-compatible migration of pre-project conversations. Each conversation has a stable AvatarSeed (stored as a long but generated within a JS-safe uint32 range) and optional Pattern/Palette overrides that mirror project-level overrides — project overrides take precedence for shared sequences, while conversation overrides apply to standalone chats. Summary and SummarizedThroughMessageId provide a rolling conflated history that the history assembler can prepend as a system message (allowing older messages to be dropped to bound provider context). Conversation.StateJson stores a serialized ConversationState in a JSON column because the shape evolves and its fields are not queried directly.

The class exposes Messages as an IReadOnlyList backed by a private `List<Message>`; properties have private setters, so callers should modify a conversation via the entity's methods (e.g., Add/Remove message, SetState, SetMode) or via EF Core during persistence. EF Core requires the private parameterless constructor — do not use it directly in application code.

## Example
```csharp
var userId = Guid.Parse("...user id...");
var projectId = Guid.Parse("...project id...");
var conversation = Conversation.Create(userId, projectId, title: "Planning chat");

Console.WriteLine(conversation.Id);        // unique conversation id
Console.WriteLine(conversation.Title);     // "Planning chat" or Id string when title omitted
Console.WriteLine(conversation.AvatarSeed); // stable seed used by client avatar rendering
```

## Notes
- Conversation.Create validates that userId and projectId are non-empty GUIDs and throws ArgumentException for empty values.
- Existing pre-migration conversations may have a null ProjectId; newly created conversations will have a non-null ProjectId.
- Title defaults to the conversation Id string when the caller supplies null/whitespace; callers can rename via the public API (PATCH/rename method) rather than changing the property directly.
- The entity is not inherently thread-safe; concurrent mutations should be synchronized by callers or coordinated through repository/transaction boundaries.
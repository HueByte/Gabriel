# Conversation

> **File:** `src/api/Gabriel.Core/Entities/Conversation.cs`  
> **Kind:** class

Represents a user-scoped chat thread with its metadata and owned messages. Reach for this entity when creating, persisting or manipulating a conversation (aggregate root) — repositories filter by UserId so each user only sees their own conversations.

## Remarks
Conversation is the aggregate root that contains a readonly collection of Message instances and stores conversation-level data used by UI and provider logic: stable avatar seed, optional pattern/palette overrides, a rolling Summary used to truncate provider input, and a JSON blob (StateJson) for evolving per-conversation state. The class uses private setters and a parameterless constructor to support EF Core; mutation is intended to happen via the class' domain methods (Create is provided). ProjectId is nullable on the entity to support a backfill/migration path where existing conversations are later assigned to a user's default project.

## Example
```csharp
var conversation = Conversation.Create(userId: user.Id, projectId: project.Id, title: "Brainstorm");
Console.WriteLine(conversation.Title);
// Access messages (read-only view of the internal list)
foreach (var msg in conversation.Messages)
{
    Console.WriteLine(msg.Content);
}
```

## Notes
- Repository queries should always filter by UserId; the entity is scoped per-user.
- ProjectId is nullable to support an incremental migration/backfill; new conversations are expected to have a non-null ProjectId.
- StateJson is a free-form JSON column intended for evolving shape; its fields are not directly queryable in the database.
- AvatarSeed is stored as a long but is generated within the JS-safe uint32 range so it round-trips through JSON reliably.
- Mode is nullable: null means "use default (Chatty)"; the nullable design lets adding the column be non-blocking and supports layering a user-level default in the future.
- Title defaults to the conversation Id when no title is supplied. CreatedAt/UpdatedAt are set to UtcNow by default; updating UpdatedAt is expected to be handled by domain methods or persistence logic.
```
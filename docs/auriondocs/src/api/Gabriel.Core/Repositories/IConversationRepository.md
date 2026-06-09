# IConversationRepository

> **File:** `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`  
> **Kind:** interface

Repository abstraction for storing and querying Conversation aggregates in a user-scoped way. Use this interface when reading conversations that must be restricted to a specific owner, listing a user's conversations (optionally filtered by project), and performing create/update/delete operations on conversations and their messages without exposing persistence details.

## Remarks
Read operations on this repository are intentionally user-scoped: every read takes a userId so the implementation can refuse cross-tenant access and prevent accidental leakage of another user's conversations. Write methods do not take a userId because ownership is carried on the Conversation entity itself and is expected to have been established by a prior user-scoped read (EF's change tracker then associates the change with the correct owner). The interface exposes both a GetByIdAsync (without messages) and GetByIdWithMessagesAsync (eagerly includes messages) to avoid unnecessary loading. RemoveMessages exists as an explicit method so EF's change tracker will reliably mark message rows for deletion across EF versions/configurations.

## Example
```csharp
// Typical usage pattern: load a conversation for the current user, add a message, mark the aggregate updated.
var conversation = await repo.GetByIdWithMessagesAsync(conversationId, currentUserId, cancellationToken);
if (conversation is null) return NotFound();

var message = new Message { Text = "Hello", SenderId = currentUserId, CreatedAt = DateTime.UtcNow };
// Attach the message to the aggregate and inform the repository so EF tracks the new row
conversation.Messages.Add(message);
repo.AddMessage(message);
repo.Update(conversation);
// Persisting changes (SaveChangesAsync) is the caller's responsibility / handled by the unit-of-work.
```

## Notes
- Always pass the correct userId to read methods; otherwise the repository will return null or refuse access.
- AddMessage is synchronous (void) while adding a Conversation uses AddAsync — this reflects intent to enqueue a new message on an already-tracked aggregate rather than create a new aggregate root.
- Use RemoveMessages to delete message rows so EF's change tracker reliably marks them for deletion; removing messages only from the navigation collection may not produce deletes in all EF configurations.
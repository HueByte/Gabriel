# IConversationRepository

> **File:** `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`  
> **Kind:** interface

```csharp
public interface IConversationRepository
```


IConversationRepository provides tenant-scoped data access for Conversation aggregates, exposing read, list, and write operations to create, update, and delete conversations and their messages. It enforces ownership boundaries by requiring user context on reads, and it centralizes persistence concerns behind a repository boundary rather than exposing raw EF calls.

## Remarks
By centralizing data access, this interface enforces the ownership model at the boundary between domain logic and persistence. The separate GetByIdAsync and GetByIdWithMessagesAsync methods make it explicit whether message history is loaded with the conversation. AddAsync persists new Conversation entities, while AddMessage supports inserting a Message tied to an existing Conversation. RemoveMessages provides explicit deletion semantics to ensure the EF change tracker marks removals reliably across provider configurations.

## Example
```csharp
// Example: load a conversation with its messages for the current user
var convo = await repo.GetByIdWithMessagesAsync(convoId, userId, ct);

// Example: list conversations for a user (optionally filtered by project)
var list = await repo.ListAsync(userId, projectId, ct);

// Example: create and persist a new conversation (ownership modeled on the entity itself)
var newConvo = new Conversation { /* initialize properties, including ownership */ };
await repo.AddAsync(newConvo, ct);
```

## Notes
- Reads are tenant-scoped; always supply userId to prevent cross-tenant data exposure.
- Update/Remove typically require the entity to have been loaded via a user-scoped read to ensure ownership is respected.
- RemoveMessages expects the exact set of messages to delete; use carefully to avoid unintended deletions.

# ConversationRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`  
> **Kind:** class

```csharp
public class ConversationRepository : IConversationRepository
```


A concrete EF Core repository that implements IConversationRepository and provides CRUD-style access to Conversation and Message entities using an AppDbContext. Use this repository from application services when you need to load, add, update or remove conversations or their messages while keeping persistence concerns encapsulated.

## Remarks
This class sits at the persistence boundary and encapsulates common queries and mutations for conversations: fetching by id (optionally including messages), listing a user's conversations (optionally filtered by project), and adding/updating/removing entities. It intentionally performs only change tracking and query composition — it does not commit changes to the database itself, leaving transaction and save semantics to the caller or a surrounding unit-of-work.

## Example
```csharp
// typical usage from an application service
var conversation = await repo.GetByIdWithMessagesAsync(conversationId, userId, ct);
if (conversation is null) throw new InvalidOperationException("Conversation not found");

var message = new Message { ConversationId = conversation.Id, Content = "Hello", CreatedAt = DateTime.UtcNow };
repo.AddMessage(message);
conversation.UpdatedAt = DateTime.UtcNow;
repo.Update(conversation);

// Persist changes via the DbContext or unit-of-work that manages transactions
await dbContext.SaveChangesAsync(ct);
```

## Notes
- The repository methods (AddAsync, AddMessage, Update, Remove, RemoveMessages) only mutate the DbContext's change tracker; callers must call SaveChanges/SaveChangesAsync (or use a unit-of-work) to persist changes.
- GetByIdWithMessagesAsync includes the Messages navigation and applies an OrderBy on CreatedAt in the query. Depending on EF Core version and how the results are materialized, callers who require a guaranteed ordering in-memory should explicitly order the Messages collection after materialization to avoid relying on provider-specific behavior.
- AddAsync returns a Task because it delegates to EF Core's AddAsync; the method does not return the added entity or an EntityEntry — callers should rely on the supplied entity instance which will be tracked after the call.
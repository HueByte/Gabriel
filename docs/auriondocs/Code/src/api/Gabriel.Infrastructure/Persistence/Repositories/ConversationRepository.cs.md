# ConversationRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`  
> **Kind:** class

```csharp
public class ConversationRepository : IConversationRepository
```


Provides data access for Conversation and Message entities using an AppDbContext-backed implementation of IConversationRepository. Use this repository from the infrastructure/data layer when you need to query, add, update, or remove conversations and their messages while keeping queries scoped to a specific user (userId) and, optionally, a project (projectId).

## Remarks
This class centralizes common persistence patterns for conversations: fetching a single conversation (optionally with its messages), listing a user's conversations (with an optional project filter), and performing add/update/remove operations against the EF Core change tracker. Query methods apply the userId filter to ensure callers only retrieve their own conversations, and GetByIdWithMessagesAsync attempts to include messages ordered by CreatedAt so consumers receive messages in creation order.

## Notes
- Repository methods modify the DbContext's change tracker but do not persist changes. Call SaveChanges/SaveChangesAsync on the AppDbContext (or a unit-of-work wrapper) to commit inserts/updates/deletes to the database.
- AppDbContext (DbContext) is not thread-safe; do not share a single ConversationRepository/DbContext instance concurrently across threads.
- The ordering applied inside Include (Messages.OrderBy(m => m.CreatedAt)) expresses the intended order, but actual materialized ordering can vary by EF Core provider/version; if a strict ordering is required, also order the collection when projecting or after materialization.
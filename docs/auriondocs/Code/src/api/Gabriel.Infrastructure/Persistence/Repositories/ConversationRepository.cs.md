# ConversationRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`  
> **Kind:** class

Provides data access for Conversation aggregates using an AppDbContext-backed EF Core repository. Use this class when you need to query, add, update or remove conversations and their messages while scoping queries to a specific user (and optionally a project); it does not commit changes — callers must save the DbContext.

## Remarks
Implements the IConversationRepository abstraction over an AppDbContext to centralize commonly used queries and persistence operations for conversations and messages. Queries are consistently filtered by userId (and optionally projectId) to keep data scoped to the requesting user. Mutating methods (AddAsync, AddMessage, RemoveMessages, Update, Remove) only modify the EF change tracker; persisting those changes is left to the caller (for example via SaveChangesAsync) which keeps transaction and unit-of-work control outside the repository.

## Example
```csharp
// typical usage inside an application service
public async Task CreateConversationExample(IConversationRepository repo, AppDbContext ctx, Guid userId, CancellationToken ct)
{
    var conversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, Title = "New convo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    await repo.AddAsync(conversation, ct);
    // repository does not save; persist via DbContext
    await ctx.SaveChangesAsync(ct);
}

// fetching a conversation with its messages
var convo = await repo.GetByIdWithMessagesAsync(conversationId, userId, ct);
if (convo != null)
{
    // convo.Messages should be available (ordered by CreatedAt as requested by the query expression)
}
```

## Notes
- The repository methods do not call SaveChanges/SaveChangesAsync — callers must persist the DbContext to commit changes.
- Queries are explicitly filtered by userId to enforce data scoping; ensure the correct userId is provided to avoid empty results.
- GetByIdWithMessagesAsync includes the Messages navigation and specifies an ordering by CreatedAt in the Include expression; depending on EF Core version/provider, the ordering inside Include may not always be preserved by the provider and you may need to explicitly order the loaded collection after materialization if deterministic ordering is required.

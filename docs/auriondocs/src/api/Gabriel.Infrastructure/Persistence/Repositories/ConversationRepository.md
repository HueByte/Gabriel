# ConversationRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`  
> **Kind:** class

A repository that implements IConversationRepository using an EF Core AppDbContext to load and modify Conversation and Message entities for a specific user. Use this class when you want a data-access abstraction for listing, retrieving (optionally with messages), adding, updating, or removing conversations and their messages while leaving transaction/save control to the caller.

## Remarks
This class encapsulates the EF Core queries and change-tracking operations for Conversation and Message aggregates. Query methods filter by userId (and optionally projectId) to ensure callers only access a user's conversations; ListAsync orders conversations by UpdatedAt descending. GetByIdWithMessagesAsync attempts to include the related messages and applies an ordering by CreatedAt so the returned Conversation's Messages collection is sorted. Mutating methods (AddAsync, AddMessage, RemoveMessages, Update, Remove) attach changes to the AppDbContext but do not call SaveChanges — the caller is responsible for committing the unit of work.

## Example
```csharp
// typical usage inside a service with an injected AppDbContext and ConversationRepository
var repo = new ConversationRepository(dbContext);

// add a new conversation
var conversation = new Conversation { Id = Guid.NewGuid(), UserId = userId, ProjectId = projectId };
await repo.AddAsync(conversation, ct);
await dbContext.SaveChangesAsync(ct); // persist

// fetch with messages
var loaded = await repo.GetByIdWithMessagesAsync(conversation.Id, userId, ct);
if (loaded != null)
{
    // messages (if any) should be ordered by CreatedAt
    foreach (var msg in loaded.Messages)
    {
        Console.WriteLine(msg.Text);
    }
}

// remove messages
repo.RemoveMessages(loaded.Messages.Where(m => m.IsObsolete));
await dbContext.SaveChangesAsync(ct);
```

## Notes
- GetByIdAsync and GetByIdWithMessagesAsync return null when no matching conversation exists.
- AddAsync, AddMessage, RemoveMessages, Update and Remove only modify the DbContext state; call SaveChanges/SaveChangesAsync on the AppDbContext to persist.
- The Include(...OrderBy(...)) used to request ordered messages relies on EF Core query translation; verify the produced SQL/behavior for your EF Core version/provider if precise ordering is critical.
- The repository is not thread-safe; share a repository/DbContext instance only within the intended scope (typically a single request or unit of work).

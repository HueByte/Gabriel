# ConversationRepository

> **File:** `src/api/Gabriel.Infrastructure/Persistence/Repositories/ConversationRepository.cs`  
> **Kind:** class

```csharp
public class ConversationRepository : IConversationRepository
```


A thin EF Core repository that implements IConversationRepository and encapsulates common data access patterns for Conversation and Message entities tied to a specific user. Use this class when you need user-scoped queries (by userId), to include a conversation's messages, or to add/update/remove conversations and messages through the application's AppDbContext.

## Remarks
This repository centralizes query logic and mutation helpers so callers don't duplicate filtering or include logic across the codebase. It intentionally delegates persistence (SaveChanges/SaveChangesAsync) to the caller: methods add/update/remove entities on the injected AppDbContext but do not commit changes. The class assumes a per-request or otherwise appropriately scoped DbContext (DbContext is not thread-safe).

## Notes
- These methods modify the injected AppDbContext but do not call SaveChanges/SaveChangesAsync; callers must persist changes explicitly.  
- AppDbContext (and therefore this repository) is not thread-safe — do not reuse the same instance concurrently on multiple threads.  
- GetByIdWithMessagesAsync uses an Include with an OrderBy for messages; depending on EF Core version and materialization, consumers who rely on a specific in-memory ordering should apply an explicit OrderBy when enumerating the Messages collection.
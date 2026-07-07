# IConversationRepository

> **File:** `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`  
> **Kind:** interface

```csharp
public interface IConversationRepository
```


IConversationRepository defines a tenant-aware persistence contract for conversations and their messages. Reads are scoped to the owner and exposed via GetByIdAsync and GetByIdWithMessagesAsync, while ListAsync enumerates a user’s conversations with an optional project filter. Writes do not take a userId parameter because ownership lives on the Conversation entity itself, and EF tracks changes on that entity after it has been loaded in a user-scoped read. This interface isolates persistence concerns from domain logic and provides explicit methods for adding, updating, and removing conversations and their messages.

## Remarks
This abstraction centralizes tenant isolation rules and the lifecycle of messages attached to conversations. It coordinates with EF’s change-tracking and enforces that deletions are performed explicitly through RemoveMessages to avoid fragile orphan-removal semantics across EF configurations.

## Notes
- Be mindful that AddAsync does not take a userId parameter; ensure the Conversation entity’s UserId is set to the owning user before calling AddAsync.
- RemoveMessages requires an explicit collection of Message instances to delete; relying on orphan-removal from navigation properties can be fragile across EF versions/configurations.
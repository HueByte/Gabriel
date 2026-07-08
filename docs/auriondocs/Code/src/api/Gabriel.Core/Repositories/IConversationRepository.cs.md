# IConversationRepository

> **File:** `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`  
> **Kind:** interface

```csharp
public interface IConversationRepository
```


IConversationRepository provides a tenant-aware data access contract for Conversation aggregates and their associated Message entities. It exposes read methods that require a userId to ensure a user can only retrieve their own conversations, and write methods that persist changes while respecting the ownership stored on Conversation. Use this interface when you need to fetch a conversation (with or without its messages), list a user’s conversations with an optional project filter, or perform create/update/delete operations on conversations and their messages. The explicit separation between AddMessage and RemoveMessages reflects deliberate control over message lifecycle and EF change-tracking, ensuring deletions are clearly expressed rather than inferred from navigation state.

## Remarks
This interface acts as an abstraction over the persistence layer (likely EF Core) and enforces per-user isolation by requiring userId for reads. It coordinates the Conversation and Message entities, anchoring ownership to the Conversation’s UserId and ensuring that write operations surface intent clearly (e.g., removing messages explicitly rather than relying on cascade behavior). By providing both single-conversation reads and a bulk-list capability, it supports both targeted queries and user-scoped overviews while remaining decoupled from any particular data-access implementation.

## Notes
- Be mindful that GetByIdAsync may return null; always handle the possibility of a missing conversation for the given user.
- AddAsync assumes the Conversation entity contains its ownership (UserId) prior to persistence; ensure this before calling AddAsync.
- RemoveMessages is explicit to guarantee EF’s change tracker marks deletions reliably, avoiding fragile orphan-removal logic across EF configurations.
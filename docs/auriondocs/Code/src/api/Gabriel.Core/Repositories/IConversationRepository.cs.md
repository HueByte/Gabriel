# IConversationRepository

> **File:** `src/api/Gabriel.Core/Repositories/IConversationRepository.cs`  
> **Kind:** interface

An abstraction for storing and retrieving Conversation and Message entities with enforced user-scoped reads and EF-friendly write operations. Use this repository when you need to query or modify conversations belonging to a specific user (reads require an explicit userId) while relying on the persistence layer (EF) to track ownership and perform actual database commits.

## Remarks
This interface centralizes conversation-related data access and encodes two important conventions: read operations are user-scoped (every read requires the caller to supply the owner userId so the repository refuses to return cross-tenant data), while write operations do not accept a userId because ownership is stored on the entity and tracked by the ORM. Message deletion is exposed as an explicit method to ensure the EF change tracker marks rows for deletion reliably (instead of depending solely on orphan-removal semantics which can vary across EF versions/configurations).

## Example
```csharp
// Typical read: always include the owner userId to avoid leaking another user's data
var conversation = await conversationRepo.GetByIdAsync(conversationId, currentUserId, ct);
if (conversation == null) return NotFound();

// Add a new message to an existing, user-scoped conversation
var message = new Message { ConversationId = conversation.Id, Text = "Hello", CreatedAt = DateTime.UtcNow };
conversationRepo.AddMessage(message);

// Remove messages explicitly so EF marks them for deletion
conversationRepo.RemoveMessages(conversation.Messages.Where(m => m.IsFlagged));

// Persist changes via the surrounding unit-of-work / DbContext
// await unitOfWork.SaveChangesAsync(ct); // not part of this interface
```

## Notes
- All read APIs require the caller to pass the user's Guid; failing to do so may return null even if the conversation exists for another user.
- AddAsync / AddMessage / Update / Remove are write-intent operations; they modify the tracked model but do not themselves commit to the database — call SaveChanges on your DbContext or unit-of-work to persist.
- Prefer RemoveMessages when deleting child Message entities to avoid relying on EF orphan-removal behavior that can be brittle across versions/configurations.
- ListAsync treats a null projectId as "all projects"; pass a concrete projectId to filter to a single project.
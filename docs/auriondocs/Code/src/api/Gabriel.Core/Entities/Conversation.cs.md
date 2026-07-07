# Conversation

> **File:** `src/api/Gabriel.Core/Entities/Conversation.cs`  
> **Kind:** class

```csharp
public class Conversation
```


Represents a user-scoped chat thread and its metadata: identity, owner (UserId), optional project containment, visual avatar seed/overrides, a rolling summary used for history assembly, serialized per-conversation state, interaction mode, timestamps, and the ordered message collection. Use the static Create(...) factory to construct new conversations (the parameterless ctor is reserved for EF Core), read messages via the Messages property, and use the GetState/SetState helpers to work with the JSON state blob.

## Remarks
Conversation is the aggregate root for a single chat thread. Repositories filter by UserId so each user only sees their own conversations; ProjectId is nullable to support a migration/backfill path where pre-auth conversations are later assigned to a user's default project. The Summary and SummarizedThroughMessageId fields are used when assembling provider context: the stored summary is prepended as a system message and the messages it covers are dropped so provider context remains bounded. AvatarSeed provides a stable visual identity per conversation (stored as a long but constrained to a JS-safe uint32 range), while PatternOverride/PaletteOverride allow the conversation to opt out of the seed-derived visuals (the sequence service prefers project-level overrides when present).

## Example
```csharp
var conv = Conversation.Create(userId: user.Id, projectId: project.Id, title: "Ideas for Q4");
// Add messages via repository or domain methods. Read-only view:
var messages = conv.Messages; // IReadOnlyList<Message>

// Read and write conversation state through helper methods (not a plain property):
var state = conv.GetState();
state.TurnCount += 1;
conv.SetState(state);
```

## Notes
- ProjectId is intentionally nullable for backward compatibility; some conversations may be unassigned until a lazy backfill assigns a Default project for the user.
- AvatarSeed is stored as long in the DB but constrained to the JS-safe uint32 range to ensure it round-trips through JSON and JS clients without precision loss.
- StateJson is a versioned JSON blob (ConversationState). Its shape may evolve and is not intended for direct querying — use GetState/SetState to access it.
- Mode is nullable: null means the system default (Chatty). An explicit null allows adding a future "use the user's default" behavior without colliding with an explicit Chatty value.
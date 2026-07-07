# Conversation

> **File:** `src/api/Gabriel.Core/Entities/Conversation.cs`  
> **Kind:** class

```csharp
public class Conversation
```


Represents a single user-scoped chat thread: persistent metadata, visual identity (avatar seed / overrides), summarization state and the ordered message collection. Reach for this domain entity when creating, loading or mutating conversations in business logic or repository code; use the static Create factory to construct new conversations so required invariants (UserId, ProjectId, AvatarSeed and a sensible default Title) are applied.

## Remarks
The class is tuned for an evolving production schema and UI integration. ProjectId is nullable on the entity to support a lazy migration strategy (pre-auth conversations remain project-less and are assigned a default project later). Conversation.StateJson stores a serialized ConversationState in a single JSON column because its shape evolves and the app never queries individual fields directly; callers should use the designated accessors (GetState/SetState) rather than manipulating StateJson text. Mode is nullable so a null value can mean "use the global default" without backfilling. AvatarSeed is persisted as a long for the database but is kept within a JS-safe uint32 range so it round-trips cleanly through JSON and the client-side avatar generator. The message collection is held internally as a `List<Message>` and exposed as an IReadOnlyList to prevent external mutation; EF Core requires the private parameterless constructor for materialization.

## Example
```csharp
// Create a new conversation for persistence
var userId = Guid.Parse("4d6f3b2a-...-0001");
var projectId = Guid.Parse("7a9f1c2b-...-0002");
var conv = Conversation.Create(userId, projectId, title: "Project planning chat");

Console.WriteLine(conv.Id);         // assigned on construction
Console.WriteLine(conv.Title);      // provided title (trimmed) or conv.Id if none supplied
Console.WriteLine(conv.AvatarSeed); // stable seed chosen at creation

// Messages are exposed as a read-only list; add/remove via the domain methods on Conversation (not shown here).
var messages = conv.Messages;
```

## Notes
- The Create factory throws ArgumentException for empty userId or projectId; callers must supply non-empty GUIDs.
- Existing records may have ProjectId == null due to migration/backfill; code that relies on ProjectId should handle that case.
- Do not edit StateJson directly; use the provided state accessors (GetState/SetState) so evolving shapes are handled consistently.
- Mode == null means "use the system/default mode"; an explicit Mode value overrides that.
- Messages are exposed as IReadOnlyList; mutation is controlled by Conversation's own methods to preserve invariants and keep EF Core change-tracking consistent.
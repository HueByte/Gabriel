# Conversation

> **File:** `src/api/Gabriel.Core/Entities/Conversation.cs`  
> **Kind:** class

```csharp
public class Conversation
```


Represents a single chat thread owned by a user: it contains metadata (title, timestamps, avatar seed, optional pattern/palette overrides), a collection of Message entities, optional project association, a rolling summary and summarized-through pointer, and a serialized ConversationState. Use this class when creating, reading or manipulating conversations at the domain level (repositories and services work with Conversation instances rather than raw DTOs or primitive tuples).

## Remarks
Conversation is the aggregate root for a chat thread. It owns the messages collection (exposed as an IReadOnlyList) and stores per-conversation behavioral and presentation state that the rest of the system uses to assemble provider context, visual identity, and conversational heuristics. The serialized StateJson holds evolving, non-queryable state (turn counts, mood flags, etc.) so the shape can change without schema migrations. Project-level overrides (PatternOverride/PaletteOverride) exist so project-shared sequences can use the project's skin while standalone/default-project chats fall back to the conversation's overrides.

The class is designed for use with EF Core: it has a private parameterless constructor for materialization and exposes a factory (Create) that enforces required invariants (non-empty UserId and ProjectId) and initializes stable defaults (Id, AvatarSeed, Title). Repository code is expected to filter by UserId so users only see their own conversations.

## Example
```csharp
// create a new conversation for a user and project; if title is null the Id is used
var userId = Guid.NewGuid();
var projectId = Guid.NewGuid();
var conv = Conversation.Create(userId, projectId);

Console.WriteLine(conv.Id);            // unique conversation Id
Console.WriteLine(conv.Title);         // either supplied title or conv.Id.ToString()
Console.WriteLine(conv.AvatarSeed);    // numeric seed used by the client to generate an avatar
Console.WriteLine(conv.Messages.Count); // empty at creation
```

## Notes
- Conversation.Create enforces that userId and projectId are non-empty Guids; callers must supply valid ids or catch ArgumentException.
- The ProjectId property is nullable on the entity for migration/backfill reasons, but newly created conversations (via Create) will have a non-null ProjectId.
- EF Core materialization requires the private parameterless constructor; do not remove it if using EF mapping.
- AvatarSeed is stored as a long in the database but is chosen so it stays within a JavaScript-safe uint32 range and round-trips cleanly through JSON.
- Conversation state is stored as JSON in StateJson and must be read/written through the provided helpers (GetState/SetState) to preserve evolving shape and compatibility.

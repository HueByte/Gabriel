# ConversationResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs`  
> **Kind:** record

Represents a conversation as returned by the API. Use this contract when reading conversation data from the server (either as an item in a list or as a full single-conversation response). It consolidates identity, project linkage, avatar rendering hints, timestamps and — when requested — the conversation messages.

## Remarks
This is a read-only DTO optimized for the API surface: list endpoints populate the conversation-level metadata but omit the Messages collection, while single-conversation endpoints include Messages. Several nullable fields carry legacy or contextual semantics (see Notes). The record is positional and uses value equality; treat it as an immutable snapshot of server state.

## Example
```csharp
// List endpoint: messages are not included (Messages == null)
var listItem = new ConversationResponse(
    Id: Guid.Parse("d6f8f7a4-..."),
    ProjectId: Guid.Parse("a1b2c3d4-..."),
    Title: "Shopping List",
    AvatarSeed: 42L,
    CreatedAt: DateTimeOffset.UtcNow.AddDays(-10),
    UpdatedAt: DateTimeOffset.UtcNow,
    Messages: null,
    ProjectIsDefault: false,
    EffectiveAvatarSeed: 99L,
    PatternOverride: null,
    PaletteOverride: null,
    Mode: "chatty"
);

// Single-conversation fetch: includes Messages
var single = new ConversationResponse(
    Id: Guid.NewGuid(),
    ProjectId: null, // legacy/standalone chat
    Title: "My Notes",
    AvatarSeed: 7L,
    CreatedAt: DateTimeOffset.UtcNow.AddDays(-1),
    UpdatedAt: DateTimeOffset.UtcNow,
    Messages: new List<MessageResponse> { /* ... */ },
    ProjectIsDefault: true,
    EffectiveAvatarSeed: 7L,
    PatternOverride: "stripes",
    PaletteOverride: "blue",
    Mode: null
);
```

## Notes
- Messages is null for list responses and only populated for single-conversation fetches; do not treat null as an empty conversation.
- ProjectIsDefault is nullable: it is true when the conversation belongs to the user's auto-created Default project, false for non-default projects, and null when ProjectId is null (legacy rows not backfilled).
- EffectiveAvatarSeed is the value clients should use for rendering: when the conversation is in a non-default project it reflects the parent project's AvatarSeed; otherwise it falls back to the conversation's AvatarSeed. It is null for legacy rows where ProjectId is null.
- PatternOverride and PaletteOverride apply only to standalone (default-project) chats; real project chats render the project's skin instead — these fields are echoed so a later conversion flow can carry them forward.
- Mode is a lowercased enum name representing conversation behaviour (e.g., "chatty"); null means use the server default behavior.

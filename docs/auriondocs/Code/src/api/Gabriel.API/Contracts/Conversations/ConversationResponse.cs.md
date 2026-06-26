# ConversationResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs`  
> **Kind:** record

A compact, serializable DTO representing a conversation returned by the API. Use this contract when returning either a single conversation (with its messages) or a conversation list item (without messages) from server endpoints.

## Remarks
This record models both list and detail responses: when multiple conversations are returned, the Messages property will be null to avoid payload bloat; when a single conversation is fetched, Messages contains the full message history. Several fields are nullable to support legacy rows and the transition between "standalone" conversations and conversations that belong to a Project. Avatar and appearance fields distinguish between a conversation's own settings (AvatarSeed, PatternOverride, PaletteOverride) and the effective values used for rendering when the conversation is part of a non-default project (EffectiveAvatarSeed and the project's skin). The Mode value is a lowercased enum-name string used to bias conversational behavior.

## Example
```csharp
// Detail response (single conversation fetched)
var detail = new ConversationResponse(
    Id: Guid.NewGuid(),
    ProjectId: Guid.Parse("..."),
    Title: "Planning chat",
    AvatarSeed: 42L,
    CreatedAt: DateTimeOffset.UtcNow.AddDays(-10),
    UpdatedAt: DateTimeOffset.UtcNow,
    Messages: new List<MessageResponse> { /* ... */ },
    ProjectIsDefault: false,
    EffectiveAvatarSeed: 42L,
    PatternOverride: null,
    PaletteOverride: null,
    Mode: "chatty"
);

// List response (messages omitted)
var listItem = new ConversationResponse(
    Id: Guid.NewGuid(),
    ProjectId: null,
    Title: "Quick notes",
    AvatarSeed: 7L,
    CreatedAt: DateTimeOffset.UtcNow.AddDays(-1),
    UpdatedAt: DateTimeOffset.UtcNow,
    Messages: null,
    ProjectIsDefault: null,
    EffectiveAvatarSeed: null,
    PatternOverride: "dots",
    PaletteOverride: "warm",
    Mode: null
);
```

## Notes
- Messages will be null in list endpoints; do not assume its presence without checking.
- ProjectIsDefault and EffectiveAvatarSeed can be null when ProjectId is null (legacy rows not backfilled).
- PatternOverride and PaletteOverride are only applied for standalone/default-project conversations; real-project chats render the project's skin but still echo these values for potential future conversion.
- Mode is a lowercased enum name string (null means use the default behavior).
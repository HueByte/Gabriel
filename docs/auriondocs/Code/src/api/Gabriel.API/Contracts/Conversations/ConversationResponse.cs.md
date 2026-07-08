# ConversationResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ConversationResponse.cs`  
> **Kind:** record

```csharp
public record ConversationResponse(
    Guid Id,
    Guid? ProjectId,
    string Title,
    long AvatarSeed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<MessageResponse>? Messages,
    bool? ProjectIsDefault = null,
    long? EffectiveAvatarSeed = null,
    string? PatternOverride = null,
    string? PaletteOverride = null,
    string? Mode = null
)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `Guid` | — |
| `ProjectId` | `Guid?` | — |
| `Title` | `string` | — |
| `AvatarSeed` | `long` | — |
| `CreatedAt` | `DateTimeOffset` | — |
| `UpdatedAt` | `DateTimeOffset` | — |
| `Messages` | `IReadOnlyList<MessageResponse>?` | — |
| `ProjectIsDefault` | `bool?` | `null` |
| `EffectiveAvatarSeed` | `long?` | `null` |
| `PatternOverride` | `string?` | `null` |
| `PaletteOverride` | `string?` | `null` |
| `Mode` | `string?` | `null` |


ConversationResponse is a transportable data record that encapsulates the metadata and optional message payload for a single conversation as returned by the Gabriel API. It carries the conversation’s identity (Id, Title), its project linkage (ProjectId, EffectiveAvatarSeed), theming and behavior hints (PatternOverride, PaletteOverride, Mode), avatar/seed data (AvatarSeed, CreatedAt, UpdatedAt), and the per-conversation Messages when fetching a single conversation. In list responses, Messages is null; in detail views it is populated, enabling the client to render a complete conversation thread while keeping list responses lightweight. The nullable fields ProjectIsDefault and the backfilled seeds/overrides support legacy data and project migration workflows, while Mode’s value (lowercase enum name) and defaulting behavior guide how the chat is presented by the UI.

## Remarks
This type centralizes conversation-scoped state and presentation hints in one place, enabling a consistent client rendering path across list and detail endpoints. By tying avatar skin and avatar seed to the project when applicable, it supports cohesive theming for project chats while preserving standalone chats' personalization. The nullables reflect legacy/backfill strategies and future migrations, ensuring backward compatibility without breaking the API surface.

## Notes
- Messages is populated only when fetching a single conversation; in list responses it is null.
- ProjectIsDefault is meaningful only when ProjectId is non-null; null indicates legacy rows (pre-Phase-8 backfill).
- EffectiveAvatarSeed is null when ProjectId is null; in non-null projects it equals the parent project’s AvatarSeed, otherwise it uses the conversation’s own AvatarSeed.
- PatternOverride and PaletteOverride are echoed for backward-compatibility with conversion flows; real-project chats render the project’s skin, but these fields allow future migration to carry forward styling.
- Mode is a lowercased enum name; null means the default mode ("chatty").
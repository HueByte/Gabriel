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


Represents the API payload for a conversation, capturing its identity, metadata, and rendering hints. It includes identifiers (Id, ProjectId), title, timestamps, and optional messages when fetching a single conversation. It also carries rendering and behavior hints: avatar seed information (AvatarSeed, EffectiveAvatarSeed), optional skins overrides (PatternOverride, PaletteOverride), an indicator if the conversation belongs to the default standalone project (ProjectIsDefault), and a per-conversation display mode (Mode). In list responses, Messages is null to avoid large payloads; in single-conversation fetches, the Messages collection is populated. Behavior around EffectiveAvatarSeed and overrides depends on whether the conversation resides in a real project or a standalone/default project; when ProjectId is null (legacy rows), several fields become null or are unused.

## Remarks
This abstraction centralizes the conversation payload used by the Conversations API, separating transport concerns from domain entities. It encodes how a conversation should be presented (avatars, skins) and how much content to fetch (Messages). The conditional nullability of ProjectIsDefault and EffectiveAvatarSeed reflects phased feature work and legacy compatibility, enabling clients to render correctly across both real-project and standalone conversations.

## Example
```csharp
// Typical single-conversation payload
var convo = new ConversationResponse(
  Id: Guid.Parse("d271b0a2-5f8a-4c3a-9a4e-1d2a5f2b3c4d"),
  ProjectId: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
  Title: "Sprint Planning",
  AvatarSeed: 42,
  CreatedAt: DateTimeOffset.UtcNow.AddDays(-1),
  UpdatedAt: DateTimeOffset.UtcNow,
  Messages: new List<MessageResponse>(),
  ProjectIsDefault: false,
  EffectiveAvatarSeed: 42,
  PatternOverride: null,
  PaletteOverride: null,
  Mode: "chatty"
);
```

## Notes
- Messages is null in list responses; populate only when fetching a single conversation.
- EffectiveAvatarSeed is meaningful only when ProjectId is not null; otherwise it is null (legacy rows).
- PatternOverride and PaletteOverride convey per-conversation skin hints for standalone chats; real-project chats use the project's skin instead. Mode is transmitted as a lowercase string corresponding to a per-conversation behavioral bias; if unknown, clients should fall back to the default.
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


ConversationResponse is the API-facing data container for a chat conversation payload. It aggregates the conversation’s identity (Id, Title), optional project context and avatar behavior (ProjectId, ProjectIsDefault, EffectiveAvatarSeed), avatar rendering data (AvatarSeed, PatternOverride, PaletteOverride), lifecycle timestamps (CreatedAt, UpdatedAt), and, when requested, the list of messages (Messages). It also carries a per-conversation rendering preference (Mode). In list responses, Messages is null; in a single-conversation fetch, Messages is populated. ProjectIsDefault and EffectiveAvatarSeed convey how to present the chat within or outside a project. PatternOverride and PaletteOverride enable per-conversation skin overrides for standalone chats, while real-project chats render the project’s skin. Mode encodes the desired conversational persona; null means the default (chatty). Nullability reflects legacy compatibility: if ProjectId is null (legacy rows), several fields may be null.

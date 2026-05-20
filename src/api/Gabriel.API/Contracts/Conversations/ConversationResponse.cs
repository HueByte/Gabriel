using Gabriel.API.Contracts.Messages;

namespace Gabriel.API.Contracts.Conversations;

public record ConversationResponse(
    Guid Id,
    Guid? ProjectId,
    string Title,
    long AvatarSeed,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    // Populated when fetching a single conversation; null in list responses.
    IReadOnlyList<MessageResponse>? Messages,
    // True when this conversation belongs to the user's auto-created Default
    // project (the "standalone bucket"). Null when ProjectId is null (pre-Phase-8
    // legacy rows that haven't been backfilled yet).
    bool? ProjectIsDefault = null,
    // Effective avatar seed for rendering. Equals the parent project's
    // AvatarSeed when in a non-default project (so every chat in the project
    // shares one avatar), else falls back to the conversation's own AvatarSeed
    // (standalone behavior). Null when ProjectId is null (legacy).
    long? EffectiveAvatarSeed = null,
    // Pinned avatar skin for standalone (Default-project) chats. Real-project
    // chats render the project's skin instead; these fields are still echoed
    // back to the client so a future "convert chat into project" flow can
    // carry the skin forward.
    string? PatternOverride = null,
    string? PaletteOverride = null,
    // Per-conversation behaviour bias (chatty/elaborative/concise/tutor/critic).
    // Null = use the default (chatty). Sent as lowercased enum name.
    string? Mode = null
);

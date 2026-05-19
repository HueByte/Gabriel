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
    IReadOnlyList<MessageResponse>? Messages
);

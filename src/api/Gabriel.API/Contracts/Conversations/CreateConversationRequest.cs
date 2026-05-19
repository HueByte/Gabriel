namespace Gabriel.API.Contracts.Conversations;

// `ProjectId` is optional — if absent, the conversation lands in the user's
// Default project (auto-created if it doesn't exist yet).
public record CreateConversationRequest(string? Title, Guid? ProjectId);

namespace Gabriel.API.Contracts.Conversations;

// PUT /api/conversations/{id}/mode body. Send the lowercased enum name
// (chatty / elaborative / concise / tutor / critic), or null to clear back
// to the default (treated as chatty at read time).
public sealed record SetConversationModeRequest(string? Mode);

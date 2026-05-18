namespace Gabriel.API.Contracts.Messages;

public record MessageResponse(
    Guid Id,
    string Role,                                   // "user" | "assistant" | "system" | "tool"
    string? Content,                               // nullable: assistant-with-only-tool-calls has no text
    DateTimeOffset CreatedAt,
    string? ToolCallId = null,                     // set on tool-role messages
    IReadOnlyList<MessageToolCall>? ToolCalls = null  // set on assistant messages that requested tool calls
);

public record MessageToolCall(string Id, string Name, string ArgumentsJson);

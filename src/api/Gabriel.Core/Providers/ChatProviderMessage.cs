using Gabriel.Core.Entities;

namespace Gabriel.Core.Providers;

// Transport DTO for the IChatProvider boundary. Decoupled from the Message
// entity so providers don't depend on persistence concerns.
//
// Encodes all four message shapes the OpenAI/xAI wire protocol supports:
//   - user/system:           Content set, ToolCallId/ToolCalls null
//   - assistant (text):      Content set
//   - assistant (tool calls): Content optional, ToolCalls set
//   - tool (observation):    Content set, ToolCallId set
public record ChatProviderMessage(
    MessageRole Role,
    string? Content = null,
    string? ToolCallId = null,
    IReadOnlyList<ChatProviderToolCall>? ToolCalls = null
);

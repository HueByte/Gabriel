namespace Gabriel.API.Contracts.Messages;

public record MessageResponse(
    Guid Id,
    string Role,                                   // "user" | "assistant" | "system" | "tool"
    string? Content,                               // nullable: assistant-with-only-tool-calls has no text
    DateTimeOffset CreatedAt,
    Guid VariantGroupId,                           // shared across regen siblings; equals Id for singletons
    int VariantIndex,                              // 0-based position of this message among its variant siblings (by CreatedAt)
    int VariantCount,                              // total siblings in the variant group (1 for non-regenerated turns)
    IReadOnlyList<Guid> VariantSiblingIds,         // all sibling Ids in this variant group, sorted by CreatedAt (includes self)
    string? ToolCallId = null,                     // set on tool-role messages
    IReadOnlyList<MessageToolCall>? ToolCalls = null  // set on assistant messages that requested tool calls
);

public record MessageToolCall(string Id, string Name, string ArgumentsJson);

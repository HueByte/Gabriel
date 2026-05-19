namespace Gabriel.Core.Entities;

public class Message
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ConversationId { get; private set; }
    public MessageRole Role { get; private set; }

    // Nullable for assistant messages whose only payload is tool calls.
    public string? Content { get; private set; }

    // Set on Tool-role messages — references the assistant's tool_call.id this answers.
    public string? ToolCallId { get; private set; }

    // Set on Assistant-role messages that requested tool calls. Stored as the raw
    // JSON array exactly as it goes on the wire so we can replay it verbatim.
    public string? ToolCallsJson { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Variant grouping for regenerated assistant messages. All siblings of a
    // single regenerate-able turn share one VariantGroupId. For user / system /
    // tool messages — and for assistant messages with no regenerations yet —
    // VariantGroupId equals the message's own Id (each message is its own
    // singleton group). Exactly one message per group has IsActiveVariant = true.
    public Guid VariantGroupId { get; private set; }
    public bool IsActiveVariant { get; private set; } = true;

    private Message() { }

    internal static Message Create(
        Guid conversationId,
        MessageRole role,
        string? content,
        string? toolCallId = null,
        string? toolCallsJson = null,
        Guid? variantGroupId = null)
    {
        // Per-role payload validation.
        switch (role)
        {
            case MessageRole.User:
            case MessageRole.System:
                if (string.IsNullOrWhiteSpace(content))
                    throw new ArgumentException($"{role} messages require content.", nameof(content));
                break;
            case MessageRole.Assistant:
                if (string.IsNullOrWhiteSpace(content) && string.IsNullOrEmpty(toolCallsJson))
                    throw new ArgumentException("Assistant messages need either content or tool calls.", nameof(content));
                break;
            case MessageRole.Tool:
                if (string.IsNullOrEmpty(toolCallId))
                    throw new ArgumentException("Tool messages require a tool call id.", nameof(toolCallId));
                if (content is null)
                    throw new ArgumentException("Tool messages require content (the observation).", nameof(content));
                break;
        }

        var id = Guid.NewGuid();
        return new Message
        {
            Id = id,
            ConversationId = conversationId,
            Role = role,
            Content = content,
            ToolCallId = toolCallId,
            ToolCallsJson = toolCallsJson,
            VariantGroupId = variantGroupId ?? id,
            IsActiveVariant = true,
        };
    }

    internal void MarkInactiveVariant() => IsActiveVariant = false;
    internal void MarkActiveVariant() => IsActiveVariant = true;
}

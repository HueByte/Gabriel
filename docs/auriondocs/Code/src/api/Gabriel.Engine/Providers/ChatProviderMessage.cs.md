# ChatProviderMessage

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderMessage.cs`  
> **Kind:** record

```csharp
public record ChatProviderMessage(
    MessageRole Role,
    string? Content = null,
    string? ToolCallId = null,
    IReadOnlyList<ChatProviderToolCall>? ToolCalls = null
)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Role` | [`MessageRole`](../../Gabriel.Core/Entities/MessageRole.cs.md) | — |
| `Content` | `string?` | `null` |
| `ToolCallId` | `string?` | `null` |
| `ToolCalls` | `IReadOnlyList<ChatProviderToolCall>?` | `null` |


ChatProviderMessage is a transport DTO used at the IChatProvider boundary to carry a single chat message in the wire format supported by OpenAI/xAI. It decouples from the persistence Message entity so providers exchange messages without touching domain persistence concerns and encodes all four wire shapes by combining Content, ToolCallId, and ToolCalls with a Role.

## Remarks
By centralizing the mapping from domain messages to the wire protocol, this type keeps transport concerns isolated from persistence and domain logic. It supports the four shapes described in the comments: user/system (Content set; ToolCallId/ToolCalls null), assistant (text) (Content set; ToolCallId/ToolCalls null), assistant (tool calls) (Content optional; ToolCalls set; ToolCallId null), and tool (observation) (Content set; ToolCallId set; ToolCalls null). The ToolCalls collection references ChatProviderToolCall entries to describe individual tool invocations.

## Notes
- Be mindful of nullability to express shapes; only the non-null fields matter for a given message shape.
- For tool observations, ToolCallId should be set and ToolCalls should be null.
- When representing an assistant message that includes tool calls, ToolCalls should be populated and Content may be null or set as appropriate.
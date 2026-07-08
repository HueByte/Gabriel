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


Transport DTO for the IChatProvider boundary. Decoupled from the Message entity so providers don't depend on persistence concerns. It encodes all four message shapes supported by the OpenAI/xAI wire protocol: user/system with Content set and ToolCallId/ToolCalls null; assistant (text) with Content set; assistant (tool calls) with Content optional and ToolCalls set; and tool (observation) with Content set and ToolCallId set. This record exposes Role, Content, ToolCallId, and ToolCalls to transport the message across layers while preserving the semantics required by the protocol.

## Remarks
By isolating transport data from domain entities, this type enables providers to implement cross-boundary communication without leaking persistence concerns. It acts as a serialization-friendly carrier that all IChatProvider implementations can agree on, regardless of how messages are stored or retrieved. The four shapes map directly to the OpenAI/xAI wire protocol, and ToolCalls are represented by ChatProviderToolCall, enabling tool invocation details to flow when present.

## Notes
- The shapes rely on nullability to distinguish wire formats; populate only the fields relevant to the chosen shape.
- When using the assistant-with-tool-calls shape, Content may be omitted and ToolCalls must be non-null.
- ToolCallId is used only for the tool (observation) shape; it should not be set for user/system or assistant-with-tool-calls shapes.
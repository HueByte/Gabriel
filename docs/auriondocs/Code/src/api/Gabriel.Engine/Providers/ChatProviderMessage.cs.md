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


This record serves as a transport DTO at the IChatProvider boundary, decoupling provider implementations from the domain and persistence concerns. It encodes the four message shapes supported by the OpenAI/xAI wire protocol so chat messages can be transported in a stable, wire-friendly form regardless of internal domain models. Use this symbol whenever you need to marshal messages between chat providers and the rest of the system, rather than passing the raw domain Message entity across the boundary.

Fields map to the shapes:
- user/system: Content is set; ToolCallId and ToolCalls are null
- assistant (text): Content is set
- assistant (tool calls): Content may be null, ToolCalls is set
- tool (observation): Content is set, ToolCallId is set

## Remarks
This transport DTO acts as the boundary abstraction between provider implementations and the domain, preserving serialization semantics and protecting persistence concerns. It consolidates the wire-protocol shapes into a single, stable carrier and clarifies how each combination of fields should be interpreted by the adapter surrounding the IChatProvider boundary.

## Notes
- The fields Content, ToolCallId, ToolCalls are nullable; follow the contract for each shape.
- When using tool-calls shape, provide a non-null ToolCalls collection; Content may be omitted.
- Tool (observation) messages require ToolCallId to identify the invocation.

# ChatProviderMessage

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderMessage.cs`  
> **Kind:** record

Represents a transport-level chat message used at the IChatProvider boundary. Use this record when sending or receiving messages to/from provider implementations — it is intentionally decoupled from the persistence Message entity and models the four message shapes used by OpenAI/xAI-style wire protocols.

## Remarks
This record exists to separate provider-facing message semantics from any storage-specific concerns. It encodes the four protocol-supported shapes (user/system messages, assistant text, assistant tool-calls, and tool observations) using a small set of nullable properties so providers can inspect only the fields they need. The design favors immutability and a clear mapping to wire semantics: Role determines the origin, Content carries free-form text (optional for assistant tool-call invocations), ToolCalls carries the list of tool call frames when the assistant invokes tools, and ToolCallId links tool observations back to the originating call.

## Example
```csharp
// User or system message
var userMsg = new ChatProviderMessage(MessageRole.User, Content: "What's the weather like?");

// Assistant textual reply
var assistantText = new ChatProviderMessage(MessageRole.Assistant, Content: "It's sunny and 72°F.");

// Assistant invoking tools (content may be null or provide a prompt)
var toolCall = new ChatProviderToolCall("weather", new Dictionary<string, object?> { ["location"] = "Seattle" });
var assistantToolCall = new ChatProviderMessage(
    MessageRole.Assistant,
    Content: null,
    ToolCalls: new List<ChatProviderToolCall> { toolCall }
);

// Tool observation / tool response referencing the call id
var toolObservation = new ChatProviderMessage(
    MessageRole.Tool,
    Content: "Observed: rain expected",
    ToolCallId: "call-123"
);
```

## Notes
- Treat ToolCalls being null as the signal that no tool invocation occurred; prefer null (not an empty list) when no tool frames apply. 
- Content is optional for assistant messages that use ToolCalls; consumers must handle missing Content.
- ToolCallId is intended to associate tool-originated observations with a specific tool call; don't assume it's populated for non-tool roles.
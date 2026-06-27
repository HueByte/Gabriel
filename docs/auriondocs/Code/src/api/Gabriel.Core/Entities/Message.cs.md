# Message

> **File:** `src/api/Gabriel.Core/Entities/Message.cs`  
> **Kind:** class

Represents a single turn in a conversation, including role (User/System/Assistant/Tool), the message payload, tool-call metadata, an optional reasoning/thinking stream, and regeneration variant grouping. Create instances via the internal Create(...) factory which enforces per-role validation rules and populates identifiers and timestamps.

## Remarks
Message centralizes the data needed to persist and replay a conversational turn. Assistant messages may carry either human-visible content or a raw JSON array of tool calls (ToolCallsJson) so the system can replay the exact tool invocation sequence; tool messages reference the corresponding tool call via ToolCallId and carry the observation as Content. VariantGroupId and IsActiveVariant let the system group regenerated assistant responses and mark which variant should be treated as the active answer. Most properties have private setters to keep instances stable outside the assembly; limited mutators (SetReasoningContent, MarkActiveVariant/MarkInactiveVariant) are provided for controlled updates.

## Example
```csharp
// Create a user message
var userMsg = Message.Create(conversationId: convId, role: MessageRole.User, content: "Hello, who won the game?");

// Create an assistant message that requested tool calls (ToolCallsJson stored verbatim)
var assistantRequest = Message.Create(
    conversationId: convId,
    role: MessageRole.Assistant,
    content: null,
    toolCallsJson: "[{ \"tool\": \"score_lookup\", \"args\": { ... } }]"
);

// Create a tool observation message that answers a specific tool call
var toolObservation = Message.Create(
    conversationId: convId,
    role: MessageRole.Tool,
    content: "The final score was 3-2.",
    toolCallId: "assistant-tool-call-id-123"
);

// Mark variants and attach reasoning content captured by an agent loop
assistantRequest.SetReasoningContent("Intermediate chain-of-thought text...");
assistantRequest.MarkInactiveVariant();
assistantRequest.MarkActiveVariant();
```

## Notes
- Create(...) enforces per-role validation and throws ArgumentException for invalid payloads: User/System require non-empty Content; Assistant requires either Content or ToolCallsJson; Tool requires a non-null Content and a ToolCallId.
- ToolCallsJson is stored verbatim (raw JSON array) so callers should ensure it is well-formed and safe to persist/replay.
- Instances are not synchronized for concurrent mutation; callers should handle synchronization if messages may be modified from multiple threads.
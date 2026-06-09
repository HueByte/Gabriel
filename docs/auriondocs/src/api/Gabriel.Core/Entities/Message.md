# Message

> **File:** `src/api/Gabriel.Core/Entities/Message.cs`  
> **Kind:** class

Represents a single turn (message) in a conversation and its associated metadata — role (User, System, Assistant, Tool), content, tool-call linkage, reasoning stream, timestamps, and variant-grouping for regenerated assistant responses. Create instances via Message.Create (internal) and use the provided methods to toggle active variants or attach reasoning content; properties are otherwise read-only to callers.

## Remarks
Message centralizes several concerns for conversation persistence: role-aware payload validation (different requirements for user/system/assistant/tool messages), storage of raw tool-call JSON so calls can be replayed exactly, and a separate ReasoningContent field to capture provider-specific “thinking” streams without polluting the persisted visible content. VariantGroupId and IsActiveVariant provide a lightweight way to group regenerated assistant turns so the system can mark one sibling as the active reply while keeping earlier alternatives.

## Example
```csharp
// Inside the same assembly (Create is internal):
var userMessage = Message.Create(conversationId, MessageRole.User, "What's the weather?");

var assistantWithContent = Message.Create(conversationId, MessageRole.Assistant, "It will be sunny today.");

// Assistant that initiated tool calls; ToolCallsJson should be the raw JSON array sent/received
var assistantWithToolCalls = Message.Create(conversationId, MessageRole.Assistant, null, null, "[ { \"name\": \"get_weather\", \"arguments\": { ... } } ]");

// Tool message answering a tool call (toolCallId links back to the assistant's tool_call.id)
var toolResponse = Message.Create(conversationId, MessageRole.Tool, "Clear skies and 72 F", toolCallId: "tool-call-123");

// Mark variants when managing regenerated assistant responses
assistantWithContent.MarkInactiveVariant();
assistantWithToolCalls.MarkActiveVariant();

// Attach reasoning content captured by a provider's reasoning channel
assistantWithContent.SetReasoningContent("chain-of-thought details...");
```

## Notes
- Message.Create is internal: instances are intended to be constructed by in-assembly factories or repositories, not by external consumers.
- Validation rules are role-specific: User/System require non-empty, non-whitespace content; Assistant requires either non-whitespace content or a non-empty ToolCallsJson; Tool requires a non-empty toolCallId and non-null content (empty string content is allowed, but null is not).
- CreatedAt is set to DateTimeOffset.UtcNow when the instance is created.
- VariantGroupId defaults to the message's own Id unless a group id is supplied; this class does not enforce that exactly one message per group is active — higher-level logic is expected to maintain that invariant.
- ToolCallsJson is stored verbatim; callers must ensure it contains valid JSON in the expected shape if they intend to replay it.

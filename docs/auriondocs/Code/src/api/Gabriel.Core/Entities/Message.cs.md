# Message

> **File:** `src/api/Gabriel.Core/Entities/Message.cs`  
> **Kind:** class

```csharp
public class Message
```


Represents a single turn (or observation) in a conversation and carries role-specific payloads used by the chat/agent system. Use this entity when persisting or passing conversation messages between components; instances are created via the internal Create factory which applies per-role validation and initializes identity/metadata.

## Remarks
This class centralizes the shape and invariants of messages produced by users, system, assistants, and tools. It distinguishes several payload scenarios: plain textual content, tool-call references, assistant-initiated tool call requests (stored as raw JSON), and an optional "reasoning" stream captured separately from the visible content. VariantGroupId and IsActiveVariant provide a lightweight way to group regenerated assistant responses so a single logical turn can have multiple candidate variants; Create defaults the group to the message's own Id so non-regenerated messages are singleton groups.

## Example
```csharp
// Create a user message (content required)
var userMessage = Message.Create(conversationId: convoId, role: MessageRole.User, content: "What's the weather?");

// Create an assistant message that requested tool calls (toolCallsJson present)
var assistantWithToolRequest = Message.Create(
    conversationId: convoId,
    role: MessageRole.Assistant,
    content: null,
    toolCallsJson: "[{ \"name\": \"getWeather\", \"args\": {...} }]"
);

// Create a tool observation (requires toolCallId and content)
var toolObservation = Message.Create(
    conversationId: convoId,
    role: MessageRole.Tool,
    content: "Sunny, 72 F",
    toolCallId: someToolCallId
);
```

## Notes
- Create enforces role-specific payload rules and throws ArgumentException when requirements are not met (e.g., User/System require non-empty content; Tool requires both toolCallId and content; Assistant requires either content or toolCallsJson).
- Id and CreatedAt are set automatically; VariantGroupId defaults to the new Id when not provided.
- SetReasoningContent normalizes empty strings to null; reasoning is stored separately from Content so UIs can render a thinking/reasoning panel without altering the persisted answer body.
- MarkActiveVariant / MarkInactiveVariant only toggle the boolean on this instance; the invariant "exactly one active variant per VariantGroupId" is implied by the model but not enforced by this type across multiple Message instances (repository or caller code must ensure group-level uniqueness).
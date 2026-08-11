# Message

> **File:** `src/api/Gabriel.Core/Entities/Message.cs`  
> **Kind:** class

```csharp
public class Message
```


Represents a single turn in a conversation, carrying the role (user, assistant, system, tool), payload and any tool-call or reasoning metadata. Construction and mutation are intentionally assembly-restricted: the constructor is private and the factory/mutators are internal, so callers outside the defining assembly can read message data but not create or change messages directly. Use Message.Create(...) (from the same assembly) to build a message where per-role validation is enforced.

## Remarks
This type centralizes all data the system needs to persist and replay a conversation turn: the core text content, references to tool calls (ToolCallId and the raw ToolCallsJson), an optional separate reasoning stream, timestamps, and variant-grouping semantics used for regenerated assistant responses. VariantGroupId groups messages that are different candidate responses for the same assistant turn; for non-regenerated messages the group id equals the message id, and exactly one message in a group has IsActiveVariant = true.

ReasoningContent is stored separately from Content so UI and persistence can show an assistant's "thinking" stream without polluting the canonical answer body. ToolCallsJson is stored verbatim (the raw JSON array) so the system can replay the assistant's requested tool calls exactly as they appeared on the wire.

## Example
```csharp
var conversationId = Guid.NewGuid();

// Create a user message (requires non-whitespace content)
var userMessage = Message.Create(conversationId, MessageRole.User, "What's the weather?");

// Create an assistant message that contains content
var assistantMessage = Message.Create(
    conversationId,
    MessageRole.Assistant,
    "The weather is sunny.");

// Create an assistant message that issued tool calls (ToolCallsJson is the raw JSON array)
var assistantWithToolCalls = Message.Create(
    conversationId,
    MessageRole.Assistant,
    content: null,
    toolCallsJson: "[{\"name\":\"search\",\"arguments\":{}}]"
);

// Mark variants active/inactive (internal; shown here for callers in the same assembly)
assistantMessage.MarkInactiveVariant();
assistantWithToolCalls.MarkActiveVariant();

// Attach a captured reasoning stream (internal setter)
assistantMessage.SetReasoningContent("Step 1: check cache; Step 2: query API...");
```

## Notes
- Per-role validation is subtle: User/System messages reject content that is null/empty/whitespace, while Assistant messages accept either non-whitespace content or a non-empty ToolCallsJson. Tool messages require a non-empty ToolCallId and require content to be non-null (empty string is allowed).
- VariantGroupId defaults to the message's own Id when no variant group is supplied; pass a specific variantGroupId to group regenerated assistant messages together.
- SetReasoningContent normalizes empty strings to null (empty or null reasoning becomes null in the stored property).

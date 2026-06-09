# ChatProviderMessage

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderMessage.cs`  
> **Kind:** record

A lightweight, transport-focused representation of a chat message used by IChatProvider implementations. Use this DTO at the provider boundary when sending or receiving messages to/from model providers; it intentionally avoids persistence concerns and mirrors the four message shapes used by OpenAI/xAI-style wire protocols.

## Remarks
This record encodes the role and payload variants needed by model providers: plain text content, tool-call invocations (one or many), and tool-produced observations. It exists to decouple provider code from the application's persistence Message entity and to provide a compact, serializable shape that maps directly to provider wire formats. Being a record, instances are value-equal and intended to be immutable data carriers.

## Example
```csharp
// User message (system/user): content only
var userMessage = new ChatProviderMessage(MessageRole.User, Content: "What's the weather like today?");

// Assistant plain text reply
var assistantText = new ChatProviderMessage(MessageRole.Assistant, Content: "It's sunny and 72°F.");

// Assistant invoking tools: ToolCalls populated, content optional
var toolCall = new ChatProviderToolCall(/* ... */);
var assistantTool = new ChatProviderMessage(MessageRole.Assistant, Content: null, ToolCalls: new[] { toolCall });

// Tool observation: produced by a tool, references a ToolCallId
var toolObservation = new ChatProviderMessage(MessageRole.Tool, Content: "Sensor reading: 42", ToolCallId: "call-123");
```

## Notes
- Consumers should not assume the record enforces invariants: callers are responsible for producing the correct shape (e.g., ToolCalls populated for assistant tool-call messages, ToolCallId set for tool observations).
- ToolCalls is exposed as an IReadOnlyList to discourage mutation, but the underlying collection may still be mutable if created so—avoid mutating after construction.
- Content may be null for assistant messages that only contain tool calls; when integrating with providers, ensure your serializer preserves null vs empty string semantics as required by the wire protocol.
# Message

> **File:** `src/api/Gabriel.Core/Entities/Message.cs`  
> **Kind:** class

```csharp
public class Message
```


Represents a single turn in a conversation (user, assistant, system, or tool). Use this entity when persisting or manipulating conversation history inside the application — it centralizes role-specific payloads (content, tool call references, raw tool-calls JSON) and metadata used for regeneration and UI presentation.

## Remarks
The class enforces per-role payload rules at construction: user and system messages must have non-empty content; assistant messages must provide either content or a tool-calls JSON array; tool messages require both a toolCallId and an observation (content). Id and CreatedAt are assigned automatically. VariantGroupId groups alternative/regenerated assistant replies (defaults to the message's own Id so non-regenerated messages are their own singleton group) and IsActiveVariant marks which variant is currently active. ReasoningContent is stored separately from Content so agent "thinking" streams can be persisted and displayed independently of the main answer body; ToolCallsJson is stored verbatim to allow exact replay.

## Example
```csharp
// Create a user message (inside the same assembly where Create is accessible)
var convoId = Guid.NewGuid();
var userMsg = Message.Create(convoId, MessageRole.User, content: "What's the weather today?");

// Create an assistant message that requested tool calls (store raw tool-calls JSON)
var assistantMsg = Message.Create(
    convoId,
    MessageRole.Assistant,
    content: null,
    toolCallsJson: "[ { \"id\": \"forecast\", \"args\": { ... } } ]"
);
// Attach reasoning captured by the agent loop (empty string is normalized to null)
assistantMsg.SetReasoningContent("Searching cached forecasts and weighing recent observations...");

// Create a tool observation message that answers a specific tool call
var toolObservation = Message.Create(
    convoId,
    MessageRole.Tool,
    content: "Observed: clear skies, 18°C",
    toolCallId: "forecast-call-123"
);

// Mark a variant inactive when a regenerated reply replaces it
userMsg.MarkInactiveVariant();
```

## Notes
- Create enforces role-specific validation and throws ArgumentException for missing required payloads; callers must handle or avoid those conditions.
- Variant grouping logic (VariantGroupId / IsActiveVariant) is a coordination mechanism for consumers — the type does not enforce global invariants across multiple Message instances (e.g., ensuring exactly one active variant per group across a store).
- SetReasoningContent normalizes empty strings to null; CreatedAt is assigned with DateTimeOffset.UtcNow at construction.

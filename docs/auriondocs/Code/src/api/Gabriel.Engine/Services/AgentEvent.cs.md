# AgentEvent.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`

## Contents

- [AgentAssistantMessage](#agentassistantmessage)
- [AgentCompactDone](#agentcompactdone)
- [AgentCompactStart](#agentcompactstart)
- [AgentDone](#agentdone)
- [AgentError](#agenterror)
- [AgentEvent](#agentevent)
- [AgentReasoningDelta](#agentreasoningdelta)
- [AgentTextDelta](#agenttextdelta)
- [AgentToolCall](#agenttoolcall)
- [AgentToolResult](#agenttoolresult)
- [AgentUserMessagePersisted](#agentusermessagepersisted)

---

## AgentAssistantMessage
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentAssistantMessage(Guid MessageId, string Content, string? ReasoningContent = null) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageId` | `Guid` | — |
| `Content` | `string` | — |
| `ReasoningContent` | `string?` | `null` |


AgentAssistantMessage is a sealed, immutable record that represents the final, persisted reply from an assistant in the agent framework. It encapsulates the unique MessageId, the textual Content of the reply, and an optional ReasoningContent that contains the accumulated chain-of-thought for that turn when such data is produced. Because it derives from AgentEvent, instances can be processed, transported, and stored alongside other agent-related events in a consistent, event-driven pipeline.

## Remarks
- This abstraction separates the persisted, canonical message from transient processing state. It provides a stable object you can store or transmit as the authoritative representation of a single assistant turn.
- ReasoningContent is optional; it exists only when the provider emits a reasoning channel. When present, it preserves the internal rationales behind the reply; when absent, the system can still rely on Content and MessageId as the definitive delivery.
- Since AgentAssistantMessage derives from AgentEvent, it participates in the same event-handling semantics as other agent-originated events, enabling uniform handling, dispatch, and auditing across the system.

## Example
```csharp
// Create a persisted assistant message with reasoning data
var id = Guid.NewGuid();
var message = new AgentAssistantMessage(
    id,
    "The calculated result is 42.",
    ReasoningContent: "Reasoning: normalize inputs -> apply transformation -> derive result -> format output."
);
```

## Notes
- ReasoningContent may be null if the provider did not emit a reasoning channel; always check for null before consuming.
- If ReasoningContent is populated, be mindful of potential payload size when persisting or transporting messages.


---

## AgentCompactDone
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentCompactDone(int MessageCount, int SummaryTokens) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageCount` | `int` | — |
| `SummaryTokens` | `int` | — |


AgentCompactDone is a data-carrying event that signals the completion of a compaction pass. It reports how many messages were summarized (MessageCount) and how many tokens the summary occupies (SummaryTokens). MessageCount mirrors AgentCompactStart so the UI can render a 'summarized N messages into M tokens' line. The event is emitted only after AgentCompactStart has succeeded; if the summary step fails, AgentCompactDone is not produced, and the UI may show a longer thinking phase instead.

## Remarks
Represents the terminal signal of a compaction cycle and provides concrete metrics about its results. By carrying MessageCount and SummaryTokens as a single, immutable payload, it enables downstream components to render progress and metrics without peeking into the compaction logic. Its relationship to AgentCompactStart establishes a clear lifecycle: start, then finish, with this event emitted only on a successful finish.

---

## AgentCompactStart
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentCompactStart(int MessageCount, int CurrentTokens, int ThresholdTokens) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageCount` | `int` | — |
| `CurrentTokens` | `int` | — |
| `ThresholdTokens` | `int` | — |


AgentCompactStart is a sealed record that represents a domain event signaling that a rolling-summary compaction is about to begin. It derives from AgentEvent and carries the metrics used to coordinate the upcoming operation: MessageCount—the number of messages that will be folded, CurrentTokens—the token total before compaction, and ThresholdTokens—the token threshold that was crossed to trigger compaction. This event is emitted just before the summary provider is invoked so the UI can display a "compacting..." overlay while the system prepares the new compacted context.

## Remarks
AgentCompactStart marks a transition in the agent workflow from regular processing to the compacted-summary pathway. By packaging the three counters as a simple, immutable value, it enables observers to synchronize UI state, logging, and downstream processing without relying on side effects.

---

## AgentDone
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentDone() : AgentEvent
```


AgentDone is the terminal event in the AgentEvent hierarchy. It signals that the main loop has completed and the SSE stream will close, carrying no payload. Use this type to express completion in a strongly-typed manner instead of resorting to flags or nulls, allowing downstream consumers to pattern-match on AgentEvent and react accordingly when processing ends.

---

## AgentError
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentError(string Message) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| [`Message`](../../Gabriel.Core/Entities/Message.cs.md) | `string` | — |


AgentError represents an in-stream error that occurs during an upfront lookup phase; when a lookup fails before streaming begins, the error is thrown and surfaced to the client as an HTTP 4xx/5xx response. As a sealed record deriving from AgentEvent, it carries a single payload: the Message describing the failure. Developers reach for this type to abort the streaming workflow cleanly and convert a lookup failure into a well-formed client error, rather than continuing with a partial or invalid stream.

## Remarks
This subtype exists to separate normal event flow from error signaling. It enables pattern-based handling in the agent pipeline and centralizes the translation of lookup failures into HTTP error responses. The immutability of a record ensures the error details are preserved across asynchronous boundaries.

## Example
```csharp
// When a pre-stream lookup fails, surface a consistent error to the client
throw new AgentError("Lookup failed before streaming started");
```

## Notes
- The Message should be suitable for user-facing error messages since it may be shown to clients.
- AgentError being a record provides value-based equality, which can be useful in tests or when comparing errors.
- This error is intended to be thrown before streaming begins; do not instantiate or propagate it as a regular event during normal streaming.

---

## AgentEvent
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AgentUserMessagePersisted), "userMessagePersisted")]
[JsonDerivedType(typeof(AgentTextDelta),        "textDelta")]
[JsonDerivedType(typeof(AgentReasoningDelta),   "reasoningDelta")]
[JsonDerivedType(typeof(AgentToolCall),         "toolCall")]
[JsonDerivedType(typeof(AgentToolResult),       "toolResult")]
[JsonDerivedType(typeof(AgentAssistantMessage), "assistantMessage")]
[JsonDerivedType(typeof(AgentCompactStart),     "compactStart")]
[JsonDerivedType(typeof(AgentCompactDone),      "compactDone")]
[JsonDerivedType(typeof(AgentError),            "error")]
[JsonDerivedType(typeof(AgentDone),             "done")]
public abstract record AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `TypeDiscriminatorPropertyName` | — | `"type"` |


AgentEvent is the root of the streaming event hierarchy produced by AgentService.RunAsync and the corresponding SSE wire format. It is a polymorphic JSON envelope whose concrete event is determined by the type discriminator (the "type" property) and mapped to derived records such as AgentUserMessagePersisted, AgentTextDelta, AgentReasoningDelta, AgentToolCall, AgentToolResult, AgentAssistantMessage, AgentCompactStart, AgentCompactDone, AgentError, and AgentDone.

Developers consuming the stream read the incoming event, inspect its type, and switch on the concrete derived type to handle the payload accordingly. This design keeps the server and clients loosely coupled as new event kinds can be added without disrupting existing consumers.

## Remarks
AgentEvent abstracts the streaming protocol from the actual event payloads and defines a stable contract across the AgentService and its clients. The JSON discriminator enables forward-compatible deserialization: new event kinds can be introduced without breaking existing clients, provided they recognize the new "type" values and handle them gracefully. By centralizing the event surface under a single base type, the architecture avoids duplicating streaming logic across each event type and simplifies client-side dispatch based on the discriminator.

## Notes
- Unknown or unrecognized "type" values can lead to deserialization failures; consumers should be prepared to handle or gracefully skip such events or update to support new derived types when available.

---

## AgentReasoningDelta
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentReasoningDelta(string Delta) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Delta` | `string` | — |


Represents an incremental thinking token produced by reasoning-enabled agents. It carries the evolving chain-of-thought delta from the model, which tooling or UIs can surface in a dedicated panel and persist alongside the final assistant content as part of the agent's event stream.

## Remarks

This type provides a typed wrapper around the Delta string to distinguish reasoning content from other agent events. By inheriting from AgentEvent, it participates in the agent's event processing pipeline and can be routed to loggers, UIs, or auditors that need to inspect the agent's evolving reasoning. Use it when you want to surface, display, or persist the model's incremental reasoning steps, separate from final outputs.

## Example

```csharp
// Example: capture a reasoning delta for debugging or UI rendering
var delta = new AgentReasoningDelta("Step 1: Initialize; Step 2: Evaluate constraints; Step 3: Propose solution");
```

## Notes

- Delta data is sensitive; avoid exposing chain-of-thought publicly; apply redaction or disable in production.
- AgentReasoningDelta is immutable (record) and sealed; equality is value-based.

---

## AgentTextDelta
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentTextDelta(string Delta) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Delta` | `string` | — |


AgentTextDelta represents a single incremental token in a streaming assistant response. When the system streams text, each Delta token can be emitted as an AgentTextDelta event and concatenated by the client to reconstruct the full message; developers reach for it when implementing or consuming the streaming UI of the assistant, rather than waiting for the complete string.

## Remarks
AgentTextDelta is part of the AgentEvent event hierarchy, modeling incremental updates separate from complete messages. Its immutability and small payload (a single string Delta) enable efficient, thread-safe production and consumption of a live text stream, while preserving the order of tokens as delivered by the producer. By isolating token-level updates, this type allows clients to render partial results as they arrive and progressively build the final response.

## Example
```csharp
// Example
AgentTextDelta d1 = new AgentTextDelta("Hello ");
AgentTextDelta d2 = new AgentTextDelta("world!");
string full = string.Concat(d1.Delta, d2.Delta); // "Hello world!"
```

## Notes
- AgentTextDelta is a fragment; do not assume Delta represents a full message.
- Ensure deltas are applied in delivery order to avoid corrupted text.

---

## AgentToolCall
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentToolCall(Guid MessageId, string ToolCallId, string Name, string ArgumentsJson) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageId` | `Guid` | — |
| `ToolCallId` | `string` | — |
| [`Name`](../Providers/ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ArgumentsJson` | `string` | — |


AgentToolCall represents a request from an agent to invoke an external tool. It carries the essential context needed to route and correlate the invocation: the originating MessageId, a ToolCallId for unique identification, the tool's Name, and a JSON payload in ArgumentsJson describing the parameters.

## Remarks
AgentToolCall is a concrete member of the AgentEvent hierarchy, enabling the event stream to distinguish tool invocations from other agent actions. By anchoring the ToolCallId and ArgumentsJson to a single event, it supports reliable correlation, auditing, and subsequent handling by tooling adapters. The flexible ArgumentsJson pattern lets different tools define their own parameter schemas without altering the event type, at the expense of requiring consumers to parse and validate JSON as needed.

## Example
```csharp
// Example: creating a tool invocation event
var call = new AgentToolCall(
    MessageId: Guid.NewGuid(),
    ToolCallId: "tc-001",
    Name: "TranslateText",
    ArgumentsJson: "{ \"text\": \"Hello\", \"to\": \"es\" }"
);
```

## Notes
- Ensure ArgumentsJson is well-formed JSON before sending.
- ToolCallId should be unique per invocation to preserve traceability.

---

## AgentToolResult
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentToolResult(Guid MessageId, string ToolCallId, string Content) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageId` | `Guid` | — |
| `ToolCallId` | `string` | — |
| `Content` | `string` | — |


AgentToolResult is a concrete AgentEvent that encapsulates the output of a tool invocation performed by an agent. As a sealed record, it carries the global message identifier (MessageId), the particular tool call identifier (ToolCallId), and the tool's textual output (Content), enabling downstream components to correlate responses with their requests in an event-driven workflow.

## Remarks
By deriving from AgentEvent and grouping the three fields, this type provides a stable, immutable payload that can be routed, logged, and audited without exposing internals of the tool implementation. The MessageId allows tracing across the system, while ToolCallId preserves per-invocation correlation even when multiple tools are involved.

## Example
```csharp
var result = new AgentToolResult(Guid.NewGuid(), "tool-42", "Tool finished successfully.");
```

## Notes
- This type is immutable and uses value-based equality; avoid mutating its properties after creation.
- Be cautious about Content size and sensitivity when routing through logs or telemetry; consider truncation or redaction if necessary.


---

## AgentUserMessagePersisted
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentUserMessagePersisted(Guid MessageId) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageId` | `Guid` | — |


AgentUserMessagePersisted is a domain event emitted when the server has successfully persisted a user-originated message during RunAsync (not RegenerateAsync). It communicates the real database MessageId back to the client so the temporary client-side id (tmp-xxxxx) can be swapped with the authoritative id in place, avoiding an extra GET-conversation round-trip after streaming completes.

## Remarks
This event serves as a lightweight synchronization signal between persistence and the client UI. By exposing the authoritative MessageId as soon as persistence finishes, it decouples the ID-mapping logic from higher-level streaming flow, enabling a smooth, real-time UX without requiring additional round-trips. It fits into the AgentEvent workflow as a concrete payload that informs clients about persistence results without introducing complex transport semantics.

## Example
```csharp
// After persisting the user's message and obtaining its real ID from the database
Guid persistedMessageId = /* obtained from database */ Guid.NewGuid();
var evt = new AgentUserMessagePersisted(persistedMessageId);
// Publish 'evt' to the client stream (exact mechanism depends on the surrounding infrastructure)
```

## Notes
- Emit this event only for messages persisted during a RunAsync turn originating from a user message; RegenerateAsync flows have different semantics.
- The MessageId conveyed by this event is the durable DB identifier, not a temporary client-side id; use it to update client state accordingly.


---
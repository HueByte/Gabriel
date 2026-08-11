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


Represents a single final assistant message produced by the Agent and published as an event in the Gabriel engine. It persists the assistant's textual content and, when available, the accompanying reasoning transcript for that turn. The MessageId provides a stable identity for the message; Content stores the visible reply, while ReasoningContent (when present) contains the accumulated chain-of-thought; null indicates that the provider did not emit a reasoning channel. Derived from AgentEvent, this record participates in the engine's event stream and serves as the canonical payload for persisting and streaming assistant messages, enabling clients to reconcile a delta-updated view against the persisted canonical content.

## Remarks
AgentAssistantMessage is an immutable domain event that ties a concrete assistant reply to a unique identifier and an optional reasoning trace. By deriving from AgentEvent, it participates in the engine's event-based workflow and can be routed, persisted, or audited as part of the agent's conversation history. The separation between Content and ReasoningContent clarifies when full chain-of-thought data is available and ensures consumers can handle cases where only the final text is emitted.

## Example
```csharp
var message = new AgentAssistantMessage(
    MessageId: Guid.NewGuid(),
    Content: "The answer is 42.",
    ReasoningContent: "Step 1: Do X; Step 2: Do Y; Conclusion: 42."
);
```

## Notes
- ReasoningContent may be null if the provider did not emit a reasoning channel.
- MessageId must be unique per message; reusing IDs can disrupt client reconciliation of the persisted content.

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


AgentCompactDone is a sealed record that signals the completion of a compaction pass in the agent’s workflow. It carries the results: MessageCount, the number of messages that were condensed into the new rolling summary, and SummaryTokens, the token-length of that summary. It derives from AgentEvent and is emitted only after a successful AgentCompactStart, enabling the UI to render a line such as 'summarized N messages into M tokens.' If the summary step fails, this event is skipped, and the UI may show a longer thinking phase without a compact pair, which is expected.

## Remarks
>This event exists to transport the outcome of the summarization step through the engine’s event stream. By recording both the input size and the resulting token count, it enables downstream components (UI, telemetry, and dashboards) to present a concise view of the cost and impact of the summarization. Being a sealed record also enforces immutability and value-based equality, which makes it reliable for event-driven workflows and comparisons.

## Notes
- This event assumes a preceding AgentCompactStart; if observed out of sequence, consumer code should handle the anomaly gracefully to avoid inconsistent UI states.

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


AgentCompactStart is an immutable event (sealed record) that marks the beginning of the compaction step within the agent's processing pipeline. It is emitted immediately before the rolling-summary LLM call that folds the first MessageCount messages into a single summary. The event exposes three pieces of context: MessageCount—the number of messages to be compacted; CurrentTokens—the token count before compaction; and ThresholdTokens—the token threshold that was crossed to trigger compaction. Consumers can rely on this event to drive user-facing UX (e.g., displaying a 'compacting…' overlay) and to coordinate with the summary provider invocation.

## Remarks
This abstraction isolates the initiation of the rolling summary compaction from the rest of the processing pipeline, enabling UI and orchestration code to react in a decoupled way. Deriving from AgentEvent places it within the agent's event stream, standardizing how lifecycle moments like compaction are observed and handled.

## Example
```csharp
// Common usage: pattern-match the event from an AgentEvent stream
AgentEvent e = new AgentCompactStart(5, 10240, 12000);
if (e is AgentCompactStart s)
{
  Console.WriteLine($"Compacting first {s.MessageCount} messages; current tokens: {s.CurrentTokens}, threshold: {s.ThresholdTokens}");
}
```

## Notes
- Immutable: the sealed record makes instances value-based and thread-safe to reason about.
- The fields represent pre-compact state: CurrentTokens is the total token count before compaction; ThresholdTokens is the crossing point that triggered the event.
- This event is emitted BEFORE the summary provider call; do not rely on it for post-compact state.

---

## AgentDone
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentDone() : AgentEvent
```


AgentDone is a terminal AgentEvent that signals the end of the agent's event loop and the impending closure of the SSE stream. Implemented as a parameterless, sealed record deriving from AgentEvent, it serves as a lightweight sentinel that downstream code can pattern-match against to stop processing and perform necessary cleanup when the stream ends.

## Remarks
AgentDone sits at the end of the AgentEvent hierarchy, providing a clear, type-based signal for completion rather than using exceptions or error states. By being a distinct, payload-free record, it keeps the end-of-stream semantics lightweight and easy to reason about; downstream components can pattern-match on AgentDone to terminate processing and flush resources deterministically.

## Example
```csharp
// Example: terminate processing when the terminal event is observed
void HandleEvent(AgentEvent e)
{
    if (e is AgentDone)
    {
        // End of stream reached; stop consuming further events
        return;
    }

    // Handle other events...
}
```

## Notes
- Payloadless sentinel: AgentDone has no data, so if you need context about why the stream ended, use a separate event or attach data to another event type.

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


AgentError is a sealed record that represents an in-stream error signaling a lookup failure that occurs before streaming begins; such failures surface to the client as HTTP 4xx/5xx. It carries a single Message describing the failure and derives from AgentEvent to participate in the same event stream as other agent events.

## Remarks
AgentError isolates initialization-time failures from the normal event flow, allowing the transport layer to translate its presence into a precise HTTP status and error payload. Being a sealed record, it guarantees immutability and a stable identity for logging, correlation, and telemetry within the agent's event stream.

## Notes
- It is immutable: the Message value is assigned at construction and never changes.
- This type signals pre-stream lookup failures; if a failure occurs after streaming begins, a different AgentEvent-derived type should be used.

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


AgentEvent is the abstract base record for events yielded by AgentService.RunAsync and served over Server-Sent Events. It represents a polymorphic stream of runtime events serialized to JSON using a discriminator named type, enabling clients to switch on the concrete event kind at runtime. The supported derivatives include AgentUserMessagePersisted, AgentTextDelta, AgentReasoningDelta, AgentToolCall, AgentToolResult, AgentAssistantMessage, AgentCompactStart, AgentCompactDone, AgentError, and AgentDone. When consuming the event stream, clients deserialize each payload as AgentEvent and pattern-match on the concrete type to drive UI updates, logging, or orchestration logic.

## Remarks
This discriminated union serves as the single public contract for all streaming events emitted by AgentService. It decouples the event producers from consumers: new event shapes can be added by extending the derived types without changing existing deserialization logic, provided the discriminator string is wired. It also enables straightforward client rendering: a single stream of AgentEvent values can be pattern-matched against known derived types to render or react to the agent's turns, tool interactions, and completion states.

## Notes
- The JSON payload must include the discriminator type for proper deserialization; omitting it can cause the deserializer to fail to resolve the concrete type.
- If a new derived event is added in the future, register it with a JsonDerivedType attribute on AgentEvent so clients can deserialize successfully.
- When consuming long-running streams, consider backpressure and cancellation to avoid unbounded buffering.

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


AgentReasoningDelta is a sealed record that carries an incremental token of the model's reasoning stream as a Delta string, enabling it to flow as an AgentEvent in the engine. Use it when you need to surface or persist the evolving reasoning steps produced by reasoning-capable providers; the UI may render the Delta incrementally, while the final assistant message carries the accumulated reasoning for persistence.

## Remarks
AgentReasoningDelta exists to separate reasoning data from other events, enabling targeted handling, auditing, or redaction. By modeling it as a dedicated AgentEvent subtype, components can subscribe to or filter reasoning tokens without parsing unrelated payloads, and streaming reasoning can be composed from discrete Delta fragments. This aligns with the event-driven architecture, allowing reasoning data to travel through the same pipelines as other agent events.

## Notes
- Be mindful of privacy: exposing reasoning segments can reveal sensitive model internals; redact or avoid persisting in production.
- Delta may be large and emitted frequently; ensure consumers can handle backpressure and avoid overwhelming logs or transports.

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


AgentTextDelta is a streaming event that carries a fragment of the assistant's text. Each instance exposes the Delta string, which consumers should append to the accumulating message to reconstruct the full reply as tokens arrive.

## Remarks
AgentTextDelta is a concrete, immutable record derived from AgentEvent used in the engine's event stream to convey small text fragments. Each instance carries a Delta string that should be appended to the current output by the consumer. The pattern enables clients to render streaming responses as they are produced, without waiting for a complete message; it decouples token production from rendering and allows incremental updates to the UI.

## Notes
- AgentTextDelta is immutable; the Delta value is fixed at construction.
- In streaming scenarios, several AgentTextDelta events may arrive sequentially; consumers should concatenate their Delta values in arrival order to form the full message.

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


Represents a single tool invocation event in the agent’s processing pipeline. This immutable record carries the context required to trigger a tool—linking the originating MessageId, a unique ToolCallId, the tool name, and a JSON-encoded Arguments payload—so that tool interactions are modeled as first-class events rather than ad-hoc calls.

## Remarks
As a concrete member of the AgentEvent family, AgentToolCall enables type-based routing and stable correlation between requests and responses. The MessageId provides traceability to the originating turn, while ToolCallId offers a durable handle for matching responses. The record's immutability and value-based equality simplify caching, deduplication, and event-sourcing scenarios.

## Example
```csharp
var toolCall = new AgentToolCall(
    MessageId: Guid.NewGuid(),
    ToolCallId: "tc-001",
    Name: "SummarizeText",
    ArgumentsJson: "{ \"text\": \"Sample input to summarize\" }"
);
```

## Notes
- ArgumentsJson must contain valid JSON; downstream consumers parse this payload to construct tool invocation requests.
- ToolCallId should uniquely identify the invocation within the message context to enable reliable correlation with tool responses.

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


AgentToolResult represents the completion of an Agent Tool run. It carries the GUID of the persisted observation message (MessageId), the identifier for the specific tool invocation (ToolCallId), and the textual Content produced by the tool. As a sealed record deriving from AgentEvent, it participates in the engine's event stream and is consumed by downstream handlers to persist results or trigger follow-up actions.

## Remarks
AgentToolResult exists to decouple tool execution from result storage and routing. The MessageId references the persisted observation containing Content, enabling auditing and replay without duplicating payloads. The ToolCallId ties the result to the exact tool invocation, and Content holds the tool's output for display or storage. Being a sealed record provides immutability and value-based equality, aiding correctness and testing.

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


AgentUserMessagePersisted is a small, value-like event that signals the database has assigned the real MessageId to a user-originated message after persistence. It is emitted as the first event in every user turn that originates from RunAsync, and it carries the authoritative Id of the persisted message so the client can replace its temporary tmp-xxxxx identifier in the user entry with the actual database ID without causing an extra GET-conversation round trip.

## Remarks
This event establishes an authoritative reference for the message at the very start of a user turn, enabling consistent correlation of subsequent events in the same turn. By providing the persisted MessageId up front, it reduces latency in the client experience and avoids a follow-up fetch. It also cleanly separates concerns: persistence sequencing remains server-side while the client updates its state using the real ID from this single source of truth.

## Example
```csharp
// After persisting the user's message to the database, emit this event so the client can swap IDs
var persistedEvent = new AgentUserMessagePersisted(actualDbMessageId);
```

## Notes
- This event must be the first in the user-turn stream; emitting other events first can leave the client using a temporary ID.
- It is applicable to RunAsync turns; it does not apply to RegenerateAsync flows.

---
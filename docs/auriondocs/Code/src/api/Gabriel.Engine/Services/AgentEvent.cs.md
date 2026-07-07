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


Represents a persisted final assistant message within the agent event stream. It carries a unique MessageId, the textual Content of the response, and an optional ReasoningContent containing the model's chain of thought when the provider emits it. Use this record when you need to store or transmit a completed assistant turn as an immutable event (instead of passing around raw strings) and when you want to associate a stable identifier with the message for reconciliation, auditing, or client rendering.

## Remarks
AgentAssistantMessage is an immutable, value-based event that teammates can rely on for deterministic history reconstruction. By deriving from AgentEvent, it participates in the agent's event stream and can be ordered, filtered, or archived alongside other events. The ReasoningContent is optional and only present when the provider exposes the model's reasoning; when null, consumers should treat it as absent privacy-preserving metadata.

## Example
```csharp
// Example usage showing construction of the event
var message = new AgentAssistantMessage(
    MessageId: Guid.NewGuid(),
    Content: "Here is the result you asked for.",
    ReasoningContent: null
);
```

## Notes
- ReasoningContent is optional and may be large; avoid transmitting it when not needed for diagnostics or auditing.
- This is a sealed record, so it provides value-based equality and immutability; treat it as a historical event rather than a mutable DTO.

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


Represents the successful completion of a compaction (summarization) operation. It carries two values: MessageCount, the number of messages included in the new rolling summary, and SummaryTokens, the token length of that summary. The event is raised after AgentCompactStart when the summarization succeeds, so UI can render a line such as 'summarized N messages into M tokens.' If the summary step fails, AgentCompactDone is not emitted and the UI will observe only the extended thinking phase.

## Remarks
This event marks a completed milestone in the compaction workflow and decouples the initiation signal from the completion signal. It enables consumers such as the UI, logging, and analytics to react specifically to the outcome of the summarization without inspecting the internal compaction process. The immutable record nature ensures a consistent snapshot of the results at the moment of completion, simplifying reasoning about when the operation finished.

## Notes
- AgentCompactDone is only emitted for a successful summary; it is paired with a preceding AgentCompactStart.  
- MessageCount mirrors the input count established by AgentCompactStart to present the before/after narrative to the UI.  
- Ensure the values reflect the actual results; misreporting can confuse UI and metrics.

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


Represents an event signaling that compaction of the chat history is about to begin. A rolling-summary LLM call will fold the first MessageCount messages into a single summary. It is emitted before the summary provider is invoked so the UI can display a compacting overlay while waiting for the real turn to start. CurrentTokens is the pre-compact total; ThresholdTokens is the token threshold that was crossed to trigger the compaction.

## Remarks
The symbol is part of the AgentEvent stream and serves to synchronize backend processing with frontend feedback and telemetry. By exposing the planned scope of the upcoming compaction (MessageCount) along with token pressure (CurrentTokens and ThresholdTokens), it enables the UI and monitoring layers to respond promptly without coupling to the summary logic itself.

## Example
```csharp
// Example usage: handling an event stream of AgentEvent
void HandleEvent(AgentEvent ev)
{
  switch (ev)
  {
    case AgentCompactStart s:
      Console.WriteLine($"Compacting first {s.MessageCount} messages (tokens {s.CurrentTokens} → {s.ThresholdTokens}).");
      // Trigger a UI overlay or progress indicator here
      break;
    // other cases...
  }
}
```

## Notes
- This event represents a pre-compact state; the actual compaction result is produced by subsequent events.
- AgentCompactStart is immutable (a record); treat instances as read-only data passed through the event pipeline.


---

## AgentDone
> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
public sealed record AgentDone() : AgentEvent
```


AgentDone is a terminal event in the AgentEvent hierarchy that signals the end of the processing loop and the imminent closure of the SSE stream. It serves as a lightweight, strongly-typed sentinel so consumers can gracefully stop processing without relying on nulls or ambiguous flags.

## Remarks
Modeling termination as its own sealed record makes termination explicit and pattern-friendly: readers can switch on the event type to trigger cleanup without inspecting payloads. The empty payload reinforces that this signal carries no associated data; if you need additional context about why termination occurred, introduce a dedicated event type with payload.

## Example
```csharp
// Common termination handling
if (ev is AgentDone)
{
    // Close streams, dispose resources, exit loop
    break;
}
```

## Notes
- It carries no payload; if you need data about the termination, define a separate event with data fields.

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


Represents an in-stream error event carried by the agent's event stream. It exposes a single Message describing the failure encountered during a streaming operation and inherits from AgentEvent to participate in the shared event-handling pipeline. Use this concrete record to signal lookup or streaming failures without throwing, so downstream consumers can translate the condition into HTTP 4xx/5xx responses and apply consistent error handling.

## Remarks
AgentError sits in the AgentEvent hierarchy as an explicit, typed signal of failure within the streaming path. Modeling errors as events enables uniform logging, routing, and resilience strategies without blurring control flow with exceptions. The Message field provides contextual detail for diagnostics while keeping errors data-driven and easy to pattern-match.

## Example
```csharp
// Example usage: emit an error when a lookup fails during streaming
yield return new AgentError("Lookup failed for agent 42: not found");
```

## Notes
- The Message should avoid including sensitive data; consider sanitizing before emitting in production.
- Consumers must handle AgentError to avoid unhandled stream failures; treat as a failed item in the sequence.
- Because AgentError is a record with a single string property, equality is based on the Message content; this can be used in tests or dedup logic if appropriate.

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


AgentReasoningDelta represents a single incremental token of the model's reasoning (the chain-of-thought) produced during reasoning-enabled interactions. It encapsulates a Delta string and is emitted as an AgentEvent to separate the evolving thinking trail from the final output; the UI can render this Delta in a dedicated panel, and the final assistant message carries the accumulated reasoning alongside its content for persistence.

## Remarks

By modeling the reasoning trace as its own AgentEvent type, the system cleanly separates cognitive telemetry from user-facing results. This makes debugging, auditing, and advanced UI features possible without forcing the rationale into every response payload.

## Example

```csharp
// Example usage: emit an incremental reasoning token to the agent's event stream
var delta = new AgentReasoningDelta("Step 1: infer user intent; Step 2: apply constraints");
agentEventStream.Emit(delta);
```

## Notes

- Do not expose chain-of-thought to untrusted components; apply proper access controls.
- Delta strings can be large; prefer streaming/partial persistence and consider trimming.
- If you do not need the reasoning trail, you can disable emission of AgentReasoningDelta to conserve resources.

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


AgentTextDelta is a minimal, immutable piece of the agent's streaming output that carries a fragment of the current message. Each instance holds a Delta string, which clients concatenate in sequence to reconstruct the full assistant message as data arrives. Because it derives from AgentEvent, this record participates in a unified event stream that consumers can pattern-match against to handle deltas separately from other events such as completion or errors.

## Remarks
Architecturally, AgentTextDelta isolates the progressive text payload from the rest of the event types, enabling progressive rendering and responsive UI without waiting for the complete message. Pattern matching on AgentEvent makes it straightforward to render deltas on the fly or accumulate them for final assembly, depending on the consumer's needs.

## Example
```csharp
// Common usage: accumulate deltas to form the full message
StringBuilder builder = new StringBuilder();
foreach (AgentEvent e in streamedEvents)
{
    if (e is AgentTextDelta delta)
        builder.Append(delta.Delta);
}
string fullMessage = builder.ToString();
```

## Notes
- Preserve the emission order: reconstructing the message relies on applying deltas in the original delivery sequence.
- The Delta string may be empty in some edge cases; consumers should gracefully handle no-op deltas.
- If multiple producers emit deltas concurrently, synchronize processing to avoid interleaving fragments.


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


AgentToolCall represents an event emitted when the agent requests the execution of an external tool. It captures the originating assistant message via MessageId, assigns a unique ToolCallId for tracing and deduplication, identifies the tool to invoke by Name, and carries the invocation parameters as a JSON string in ArgumentsJson. As a sealed record that derives from AgentEvent, it participates in the same event stream as other agent-related events while remaining immutable and strongly typed. Use this type whenever the agent must trigger external tooling and you need to persist, correlate, and audit the tool invocation with the originating message.

## Remarks
Because it is a distinct event type, tooling orchestration can remain decoupled from the agent's decision logic. The MessageId linkage enables end-to-end traceability from the assistant's message through the tool invocation to the eventual tool result. ToolCallId helps recognize retries or duplicates across retries or replays.

## Notes
- ArgumentsJson must be valid JSON and match the contract of the named tool.
- ToolCallId must be unique per invocation; reusing it across retries can break deduplication.
- Be mindful of the size of ArgumentsJson to avoid excessive logging or storage overhead.

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


A sealed record that signals the completion of an agent tool invocation and carries the tool's output as a persisted observation. Use this when you need to propagate tool results through the agent's event stream and correlate them with the original tool call.

## Remarks
This abstraction decouples tool execution from result handling, enabling asynchronous processing and durable persistence of observations. The MessageId ties the event to the persisted content, while ToolCallId provides traceability back to the initiating tool request, supporting reliable auditing and replay scenarios.

## Notes
- Do not rely on Content alone for long-term storage; fetch the full observation via MessageId when needed.
- AgentToolResult is immutable (record); do not mutate after creation to preserve a clear event history.
- Be mindful of tool output size; large Content may affect transport and logging; consider storing large outputs in dedicated storage and keeping only a reference in Content.

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


The AgentUserMessagePersisted event is the first event in a turn that originates from a user message (RunAsync, not RegenerateAsync). It carries the real database MessageId assigned when the server persisted the message, enabling the client to replace its temporary tmp-xxxxx identifier with the canonical ID in place, without triggering a follow-up GET conversation round-trip after streaming completes.

## Remarks
This event decouples persistence from client-side state reconciliation by providing the authoritative ID up front for user-originated turns. It enables in-place ID reconciliation during streaming, ensuring subsequent updates within the same turn reference the server's persistent ID. As part of the AgentEvent family, it acts as a persistence-acknowledgement signal that downstream clients and handlers rely on to maintain a consistent view of messages without extra round-trips.

## Example
```csharp
// Most common usage: reconcile temporary and real IDs when the first user message is persisted
void HandleEvent(AgentEvent e)
{
    if (e is AgentUserMessagePersisted persisted)
    {
        // Swap the temporary client-side id with the real, server-generated ID
        TemporaryToRealIdMap.Swap(persisted.MessageId);
    }
}
```

## Notes
- The MessageId is the official, immutable DB identifier; do not rely on or reuse the temporary client-generated ID after handling this event.
- This event is emitted only for user-originated turns (RunAsync). RegenerateAsync turns do not emit AgentUserMessagePersisted.
- If multiple persisted messages arrive in quick succession, ensure the ID mapping is thread-safe and idempotent to prevent duplicate mappings or races.

---
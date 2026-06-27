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

Represents the final assistant text message that is persisted and emitted as an event. Use this record when you need the canonical assistant output for a turn (for example to store the final content or to let clients reconcile a delta-built view against a persisted authoritative message); it also optionally carries accumulated chain-of-thought / reasoning output when the provider exposes a reasoning channel.

## Remarks
This record is a concrete AgentEvent used to convey the assistant's completed response. The Content property holds the canonical, accumulated assistant text so clients that received incremental/delta updates can reconcile their view. ReasoningContent is included only when the provider emits an explicit reasoning channel; some providers will leave it null.

## Example
```csharp
// Create an assistant message with reasoning
var msgWithReasoning = new AgentAssistantMessage(
    MessageId: Guid.NewGuid(),
    Content: "Here is the final answer to your question.",
    ReasoningContent: "(chain-of-thought text or debug details)"
);

// Create an assistant message without reasoning
var msg = new AgentAssistantMessage(Guid.NewGuid(), "Final answer text");

// Typical usage: persist or publish the message so clients can reconcile incremental updates
messageStore.Save(msg);
eventBus.Publish(msg);
```

## Notes
- ReasoningContent is nullable; absence (null) means the provider did not expose a reasoning channel.
- The record is immutable and sealed; equality is value-based across its properties.
- Ensure MessageId is a non-empty Guid (e.g., Guid.NewGuid()) when creating messages to avoid collisions or ambiguous identity.

---

## AgentCompactDone

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
// Compaction finished. `SummaryTokens` is the size of the new rolling summary;
// `MessageCount` mirrors AgentCompactStart so the UI can render a "summarized
// N messages into M tokens" line. Always paired with a preceding
// AgentCompactStart - skipped entirely when the summary call fails (the UI
// then sees a long thinking phase but no compact pair, which is fine).
public sealed record AgentCompactDone(int MessageCount, int SummaryTokens) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `pair` | `the UI
// then sees a long thinking phase but no compact` | — |
| `fine` | `which is` | — |


Represents a successful compaction completion produced by an agent. Use this event when you need to signal that a rolling summary has been produced: MessageCount records how many messages were summarized (mirroring the preceding AgentCompactStart), and SummaryTokens is the size (in tokens) of the new rolling summary.

## Remarks
This record is emitted only when a compaction call completes successfully and is always intended to be paired with an earlier AgentCompactStart event so consumers (for example, a UI) can present "summarized N messages into M tokens." If the compaction fails, no AgentCompactDone is emitted — callers should not assume every AgentCompactStart will have a matching done event.

## Example
```csharp
// Create the event after a successful compaction
var done = new AgentCompactDone(MessageCount: 42, SummaryTokens: 128);

// Typical handling: pattern-match in an event processor
switch (evt)
{
    case AgentCompactDone(var count, var tokens):
        Console.WriteLine($"Compacted {count} messages into {tokens} tokens.");
        break;
    // other event cases...
}
```

## Notes
- AgentCompactDone is immutable (record) and derives from AgentEvent.
- Do not assume a matching AgentCompactDone will always follow AgentCompactStart — absence means the compaction failed or is still pending.
- SummaryTokens denotes the size of the new rolling summary (token count), not the number of tokens removed or saved.

---

## AgentCompactStart

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an event emitted immediately before the agent performs a compaction (rolling-summary) operation. Consumers subscribe to this event when they need to react to the start of compaction — for example to show a "compacting…" UI overlay or to pause operations that depend on the pre-compaction message stream.

## Remarks
Emitted before the summary provider is invoked so listeners see the impending compaction while it is running. The record carries a snapshot of three pieces of information: how many messages will be folded (MessageCount), the total token count before compaction (CurrentTokens), and the token threshold that triggered the compaction (ThresholdTokens). Treat this as a notification that compaction is about to start rather than confirmation that compaction has completed.

## Example
```csharp
// Typical handler reacting to compaction start
void OnAgentEvent(AgentEvent evt)
{
    if (evt is AgentCompactStart start)
    {
        // Show an overlay while the summary is being produced
        ui.ShowCompactingOverlay(start.MessageCount, start.CurrentTokens, start.ThresholdTokens);
    }
}
```

## Notes
- MessageCount is the number of earliest messages that will be folded into a summary, not the number of messages remaining after compaction.
- CurrentTokens and ThresholdTokens reflect the state at the moment the event was emitted (a snapshot); concurrent activity may change totals before compaction finishes.
- The record is immutable; use its positional properties directly when handling the event.


---

## AgentDone

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents the terminal AgentEvent emitted when an agent's processing loop has finished and the server-sent events (SSE) stream will be closed. Use this marker to detect end-of-stream/termination rather than relying on nulls or ad-hoc flags.

## Remarks
AgentDone is a payload-less, sealed record that subclasses AgentEvent and serves as a sentinel value. It exists to make the end-of-life condition explicit in event streams and handlers (for example, in SSE consumers or orchestration loops) so callers can perform clean-up, close connections, or transition state when the agent completes.

## Example
```csharp
// handle events from an agent event stream
void OnAgentEvent(AgentEvent evt)
{
    switch (evt)
    {
        case AgentDone:
            // close stream, release resources, update UI, etc.
            CloseSseStream();
            break;
        // handle other AgentEvent-derived types...
    }
}
```

## Notes
- AgentDone carries no data; all instances are structurally equal (records with no properties compare equal).
- It is sealed and immutable, so it can be used safely as a marker across threads, but avoid relying on reference equality to distinguish occurrences.
- Because it has no payload, include additional event types if you need to convey termination reason or metadata.

---

## AgentError

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an error emitted as part of the agent's event stream. Reach for this record when the agent needs to signal an in-stream/runtime error to consumers; it carries a human-readable Message. This is different from lookup or validation failures that occur before streaming and are surfaced as HTTP 4xx/5xx responses.

## Remarks
This record is an AgentEvent subtype used to surface error conditions inside an ongoing event stream. It provides a simple, immutable payload (Message) so stream consumers can inspect or log the error without relying on exceptions or transport-level error codes.

## Example
```csharp
// Creating and handling an AgentError from an event stream
AgentEvent ev = new AgentError("model failed during generation");
if (ev is AgentError err)
{
    Console.WriteLine(err.Message);
}
```

## Notes
- AgentError is an event value, not an exception; consumers should handle it as part of the event stream protocol.
- Failures that occur before a stream starts (e.g., lookup/validation errors) are expected to surface as HTTP 4xx/5xx responses and are not represented by AgentError.

---

## AgentEvent

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents the polymorphic event envelope produced by AgentService.RunAsync and used as the server-sent-events (SSE) wire format. The concrete event kind is encoded in the JSON discriminator property "type", so consumers read or switch on that discriminator (or pattern-match on the deserialized concrete record) to handle specific event shapes.

## Remarks
This abstract record is the JSON polymorphic root for all runtime events the agent emits (user messages, text or reasoning deltas, tool calls/results, assistant messages, compacting markers, errors, and completion). Declaring the discriminator explicitly ensures a stable, compact SSE wire format that downstream clients (including non-.NET consumers) can parse by inspecting the "type" field.

## Example
```csharp
// Deserialize a single JSON payload into the appropriate concrete AgentEvent
var json = /* JSON received from SSE */;
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
AgentEvent? evt = JsonSerializer.Deserialize<AgentEvent>(json, options);

switch (evt)
{
    case AgentUserMessagePersisted userMsg:
        // handle persisted user message
        break;
    case AgentTextDelta textDelta:
        // handle incremental text
        break;
    case AgentToolResult toolResult:
        // handle tool output
        break;
    case AgentError error:
        // handle error
        break;
    case AgentDone:
        // handle completion
        break;
    default:
        // unknown/unsupported event
        break;
}
```

## Notes
- AgentEvent is abstract — you will only see instances of the declared derived records. Do not attempt to instantiate AgentEvent directly.
- The JSON discriminator property is exactly "type"; the string values must match the JsonDerivedType identifiers used on this record. If a consumer uses a different JSON library, it must respect that discriminator naming and values.
- When adding new event kinds, register them via JsonDerivedType on the base so (de)serialization continues to produce the expected "type" values.

---

## AgentReasoningDelta

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents a single incremental "thinking" token (a text delta) produced by a reasoning-capable agent or provider. Use this event when you need to surface or stream partial chain-of-thought / intermediate reasoning output from a model rather than waiting for a final result.

## Remarks
This sealed positional record is an event type that extends AgentEvent and is intended for streaming or incremental UI surfaces that render a model's intermediate reasoning. Each instance carries one contiguous fragment of the reasoning stream; consumers typically concatenate received deltas in order to reconstruct the full thought process. It exists to separate transient, incremental reasoning output from final result events.

## Example
```csharp
// Creating a delta and handling it in an event handler
var delta = new AgentReasoningDelta("...intermediate thought fragment...");

void OnAgentEvent(AgentEvent ev)
{
    switch (ev)
    {
        case AgentReasoningDelta reasoning:
            // append to buffer shown in UI or log
            buffer.Append(reasoning.Delta);
            break;
        // handle other AgentEvent subtypes...
    }
}
```

## Notes
- Delta may contain partial or incomplete sentences; do not treat a single delta as the final output.
- Records are immutable; to represent a changed delta create a new AgentReasoningDelta instance.
- Preserve the order of received deltas — the sequence is significant when reconstructing the full reasoning stream.

---

## AgentTextDelta

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents a single incremental text fragment emitted by an agent during streaming generation. Consumers receive a sequence of AgentTextDelta events and concatenate their Delta values to reconstruct the agent's current message; use this record when handling or transporting partial/text-streamed output from the agent.

## Remarks
AgentTextDelta is a small immutable DTO used as one variant of the broader AgentEvent stream. It exists to model streaming/partial text output so clients can display or process generated text as it arrives rather than waiting for a complete message. When processing, preserve event order and append Delta values in sequence to form the evolving assistant text.

## Example
```csharp
// Assemble text from a stream of AgentEvent values
var builder = new System.Text.StringBuilder();
foreach (var evt in eventStream)
{
    if (evt is AgentTextDelta d)
    {
        builder.Append(d.Delta);
        // update UI with builder.ToString() to show progressive output
    }
}
var fullMessage = builder.ToString();
```

## Notes
- Delta is a fragment and may contain partial tokens or punctuation; do not assume it is a complete sentence or token.
- Correct reconstruction depends on processing events in the order received; out-of-order handling will produce incorrect text.
- The record is immutable (positional property Delta) — safe to share without defensive copying.

---

## AgentToolCall

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an event emitted when the assistant requests or invokes an external tool. Use this record when recording, serializing, or handling agent tool calls so the call can be correlated back to the originating assistant message via MessageId.

## Remarks
This sealed positional record is one of the AgentEvent variants and carries the minimal metadata needed to identify and replay a tool invocation: the assistant message that requested the tool (MessageId), a local ToolCallId for correlating responses, the tool's name, and the raw arguments as JSON. It is immutable and uses value-based equality (record semantics), making it suitable for logging, event streams, and deterministic testing of tool interactions.

## Example
```csharp
// Create a tool-call event referencing an assistant message
var evt = new AgentToolCall(
    messageId: persistedMessageId,
    ToolCallId: "call-123",
    Name: "search",
    ArgumentsJson: "{\"query\":\"cats\", \"limit\":10}"
);

// Pattern-match when processing events
switch (agentEvent)
{
    case AgentToolCall call:
        Console.WriteLine($"Tool {call.Name} called with args: {call.ArgumentsJson}");
        break;
}
```

## Notes
- ArgumentsJson is stored as a string and must be valid JSON for the target tool; this type does not validate or parse the JSON.
- ToolCallId uniqueness is not enforced by the record — generate or coordinate IDs externally if you need uniqueness guarantees.
- MessageId links this event to the persisted assistant message that requested the tool; consumers should resolve that GUID to the saved message if they need the original content.

---

## AgentToolResult

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an event emitted when a tool invocation finishes. The record carries the identifier of the persisted observation message (MessageId), the tool invocation identifier (ToolCallId), and the tool's textual output (Content). Use this type when publishing or handling the result of a tool call in the agent event pipeline — the actual observation has already been saved as a separate Message referenced by MessageId.

## Remarks
This sealed record inherits from AgentEvent so it can be handled polymorphically by any consumer that processes agent events. Making it a record provides value-based equality and an immutable data carrier for the tool result; sealing it prevents further derivation and keeps the event shape stable across the pipeline.

## Example
```csharp
// Constructing the event after persisting the observation message
var result = new AgentToolResult(messageId, toolCallId, toolOutput);

// Handling via pattern matching in an event handler
switch (agentEvent)
{
    case AgentToolResult toolResult:
        // toolResult.MessageId references the persisted Message entity
        // toolResult.ToolCallId ties back to the invocation
        // toolResult.Content holds the tool output
        break;
}
```

## Notes
- MessageId refers to the separate persisted Message that contains the observation (see the source comment).
- Content is a plain string and may contain large or unstructured text; consumers should parse or validate it as needed.
- ToolCallId links this result to the originating tool call; correlate using that ID when reconciling state across components.

---

## AgentUserMessagePersisted

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Emitted when the server has persisted a user-originated message; carries the authoritative database identifier (Guid) for that message. This event is sent as the first event of every turn that originated from a user message (RunAsync, not RegenerateAsync) so clients can replace their temporary client-side message id with the real server id without issuing a follow-up "GET conversation" request.

## Remarks
This record is part of the AgentEvent stream used during streaming runs. Its purpose is to let clients reconcile their optimistic/local message ids with the server-assigned database id as soon as the server has persisted the message. By emitting the real id early in the turn stream, the server avoids an extra round-trip and keeps client state in sync during the live stream.

## Example
```csharp
// received an AgentEvent stream from the server
void OnAgentEventReceived(AgentEvent e)
{
    if (e is AgentUserMessagePersisted persisted)
    {
        Guid serverMessageId = persisted.MessageId;
        // Look up the client's temporary id -> server id mapping and swap
        // (client maintains the mapping when it sent the message)
        ReplaceTemporaryMessageIdWithServerId(tempId, serverMessageId);
    }
}
```

## Notes
- This event is only emitted for turns that originated from a user message via RunAsync — it is not emitted for RegenerateAsync.
- The record contains only the persisted message's Guid; clients must retain whatever temporary id or correlation metadata they used when sending the message in order to map the server id back to the local UI entry.
- Immutable (sealed record) value meant for transport/notification; it does not perform any persistence itself.

---
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

Represents a persisted assistant message emitted by an agent: the final visible text (Content) plus an optional accumulated chain-of-thought (ReasoningContent). Use this record when handling, storing, or transmitting the assistant's final message in an event stream or conversation history, especially when clients need to reconcile a delta-built view against canonical content.

## Remarks
This sealed record is an event payload used in the agent's event flow to carry the assistant-facing output separately from any internal reasoning. Storing ReasoningContent separately lets consumers decide whether to surface, redact, or omit internal reasoning while still preserving the final assistant text for display or replay. As a C# record it provides value equality, immutability, and convenient with-expressions for creating modified copies.

## Example
```csharp
// Create a message with optional reasoning
var msg = new AgentAssistantMessage(
    MessageId: Guid.NewGuid(),
    Content: "Here is the answer to your question.",
    ReasoningContent: "(chain-of-thought omitted for UI)");

// Pattern-match or inspect
if (!string.IsNullOrEmpty(msg.ReasoningContent))
{
    Console.WriteLine("Reasoning available for auditing.");
}

// Create a modified copy (records are immutable)
var updated = msg with { Content = "Updated answer." };
```

## Notes
- ReasoningContent is nullable; always check for null or empty before consuming internal reasoning.
- Content is intended to be the final assistant-facing text — do not treat ReasoningContent as the canonical display string.
- The record is sealed and uses value semantics; use with-expressions to create edited copies rather than mutating.


---

## AgentCompactDone

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents the successful completion of an agent compaction (summary) operation. Contains the number of messages that were compacted and the size, in tokens, of the resulting rolling summary. Consumers (for example UI or telemetry) use this event to display or record lines such as "summarized N messages into M tokens" and to correlate compaction latency with the preceding AgentCompactStart event.

## Remarks
This record is emitted only when a compaction call completes successfully and is intended to be consumed alongside a previously emitted AgentCompactStart. The MessageCount value mirrors the count provided at compaction start so consumers can correlate start/end pairs; SummaryTokens reports the size of the new rolling summary (token count). If the summary call fails, no AgentCompactDone is emitted (the UI will observe a long thinking phase but no compact pair).

## Example
```csharp
// Emitting the event when compaction finishes
var done = new AgentCompactDone(MessageCount: 42, SummaryTokens: 128);
eventStream.Publish(done);

// Handling the event in a consumer (UI/telemetry)
switch (var ev in eventStream.Read())
{
    case AgentCompactDone compact:
        Console.WriteLine($"Summarized {compact.MessageCount} messages into {compact.SummaryTokens} tokens");
        break;
}
```

## Notes
- Always paired with a preceding AgentCompactStart; absence of a matching done event means the summary call did not complete successfully.  
- SummaryTokens is a token count (not bytes or characters).  
- The record is immutable; treat its properties as read-only data for logging, UI updates, or metrics.

---

## AgentCompactStart

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

```csharp
// Compaction is about to start: a rolling-summary LLM call will fold the first
// `MessageCount` messages into a single summary. Emitted before the summary
// provider call so the UI can show a "compacting…" overlay while the user
// waits for the real turn to start. `CurrentTokens` is the pre-compact total;
// `ThresholdTokens` is the trigger line we just crossed.
public sealed record AgentCompactStart(int MessageCount, int CurrentTokens, int ThresholdTokens) : AgentEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `MessageCount` | `int` | — |
| `CurrentTokens` | `int` | — |
| `ThresholdTokens` | `int` | — |


Signals that the agent is about to perform a rolling-summary compaction of recent messages. Emitted immediately before the summary-provider (LLM) call so UI and telemetry can show a "compacting…" state; carries the number of messages to fold (MessageCount), the current total tokens before compaction (CurrentTokens), and the token threshold that triggered compaction (ThresholdTokens).

## Remarks
This event is a pre-compaction notification intended for coordination and UX (for example, showing an overlay or recording that compaction is in progress). It does not indicate completion or success of the compaction operation — the actual summary/provider result will arrive later. Use it when you need to react to the start of a compaction workflow rather than its outcome.

## Example
```csharp
// Create the event when you detect the token threshold was crossed
var start = new AgentCompactStart(MessageCount: 8, CurrentTokens: 1250, ThresholdTokens: 1000);

// Handle the event (positional or property pattern matching)
void OnAgentEvent(AgentEvent ev)
{
    switch (ev)
    {
        case AgentCompactStart(var messageCount, var currentTokens, var thresholdTokens):
            // show UI overlay, record telemetry
            ShowCompactingOverlay(messageCount, currentTokens, thresholdTokens);
            break;

        case AgentCompactStart(MessageCount: var mCount, CurrentTokens: var cTokens, ThresholdTokens: var tTokens):
            // equivalent property pattern
            RecordCompactionStart(mCount, cTokens, tTokens);
            break;

        // other event handlers...
    }
}
```

## Notes
- The event is emitted before the LLM summary call — do not assume compaction has completed when you receive it.
- Values are a snapshot at the time of emission (immutable record); they won't be updated after compaction completes.
- MessageCount is the number of original messages that will be folded into the summary, not the number of resulting turns.

---

## AgentDone

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents the terminal agent event emitted when an agent's processing loop has finished; consumers of the event stream should treat this as a final signal and close the SSE connection or perform cleanup.

## Remarks
Used as a sentinel Event derived from AgentEvent to indicate that no further events will be produced. Emitted by the producer to mark normal termination of the agent loop (not an error); handlers should use it to run shutdown or resource-release logic and to stop waiting for additional events.

## Example
```csharp
// Producer: send the terminal event
var doneEvent = new AgentDone();
eventStream.Send(doneEvent);

// Consumer: handle events from the stream
switch (evt)
{
    case AgentDone:
        // final event received — stop processing and close SSE connection
        CleanupResources();
        CloseSseConnection();
        break;
    // other AgentEvent cases...
}
```

## Notes
- AgentDone carries no payload; it only signals completion. Consumers should not expect any further events after receiving it.
- Instances are value-equal (parameterless record), so comparing by equality will treat different instances as equal; do not rely on reference identity for uniqueness.
- Treat this event as final even if clients reconnect; it indicates the producing agent has finished its current run.

---

## AgentError

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an error event emitted by an agent during an active stream. The record carries a human-readable Message describing what went wrong and is produced as an AgentEvent when an error needs to be conveyed to consumers after streaming has started. Lookup failures (errors that occur before streaming begins) are not represented by this type; those throw and surface as HTTP 4xx/5xx responses instead.

## Remarks
This sealed record is a concrete subtype of AgentEvent used to transport runtime or in-stream errors back to callers without relying on transport-level failures. Because it is an immutable record it works well with pattern matching and value-based equality, and can be used in event-processing pipelines to detect and handle error conditions reported by the agent.

## Example
```csharp
// Receive an AgentEvent and handle an in-stream error
AgentEvent evt = await ReceiveNextAgentEventAsync();

switch (evt)
{
    case AgentError ae:
        Console.Error.WriteLine($"Agent error: {ae.Message}");
        break;
    // handle other AgentEvent variants...
}
```

## Notes
- Lookup failures do not become AgentError instances: they throw before streaming starts and are returned as HTTP 4xx/5xx responses.
- Being a record, AgentError is immutable and supports the `with` expression to produce modified copies if needed.

---

## AgentEvent

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents a single event produced by AgentService.RunAsync and the corresponding SSE/JSON wire format. Use this abstract base when consuming or producing agent events so consumers can rely on a stable polymorphic JSON discriminator ("type") and a fixed set of derived event shapes (text deltas, tool calls, errors, done, etc.).

## Remarks
AgentEvent is an abstract polymorphic record used as the root of a small event hierarchy. The concrete subclasses (e.g., AgentTextDelta, AgentToolCall, AgentAssistantMessage) are annotated with JsonDerivedType attributes and serialized with System.Text.Json using the property name "type" as the discriminator. This shape allows the service to stream heterogeneous events (SSE) while keeping the wire format compact and easy for clients to switch on the event "type" string or to deserialize directly into the appropriate subclass.

## Example
```csharp
// Serialize a concrete event and inspect the discriminator field
AgentEvent ev = new AgentTextDelta { /* properties omitted */ };
var json = JsonSerializer.Serialize(ev);
// json will include: { "type": "textDelta", ... }

// Consuming a stream of events: deserialize into AgentEvent then pattern-match
string incomingJson = /* from SSE or other source */;
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
AgentEvent e = JsonSerializer.Deserialize<AgentEvent>(incomingJson, options)!;

switch (e)
{
    case AgentTextDelta td:
        // handle incremental text update
        break;
    case AgentToolCall tc:
        // handle tool invocation
        break;
    case AgentError err:
        // handle error
        break;
    case AgentDone:
        // completion
        break;
}
```

## Notes
- The JSON discriminator property name is exactly "type" (as declared by JsonPolymorphic). Deserialization depends on the string values provided in the JsonDerivedType attributes (e.g., "textDelta", "toolCall").
- AgentEvent is abstract; instantiate and emit one of the concrete derived records instead.
- Proper System.Text.Json support for polymorphic attributes requires a runtime that respects the JsonPolymorphic/JsonDerivedType annotations (typically .NET 7+). If custom JsonSerializerOptions are used, ensure they do not disable attribute-based polymorphism.

---

## AgentReasoningDelta

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an incremental "thinking" token emitted by a reasoning-capable provider. Use this record when consumers need to receive or render partial chain-of-thought output as it is produced (for streaming UI updates or progressive logging) rather than waiting for a final, complete message.

## Remarks
AgentReasoningDelta is a sealed record that inherits from AgentEvent and carries a single string property, Delta, which contains a fragment of the model's intermediate reasoning. Providers may emit multiple AgentReasoningDelta instances in sequence to represent a streamed chain-of-thought; consumers should concatenate or process these fragments in order. Because it is an immutable record, value semantics (equality by contents) and thread-safety for read-only access are provided.

## Example
```csharp
// Receive deltas from a stream and build the full reasoning text
var reasoningBuilder = new System.Text.StringBuilder();
// imagine deltas is IEnumerable<AgentReasoningDelta> coming from the provider
foreach (var delta in deltas)
{
    Console.Write(delta.Delta); // update UI progressively
    reasoningBuilder.Append(delta.Delta);
}
var fullReasoning = reasoningBuilder.ToString();
```

## Notes
- Delta values are typically partial fragments and should not be assumed to contain a complete thought on their own.
- Order of arrival matters: assemble fragments in the order received to reconstruct the intended reasoning.
- The record is immutable; to modify or accumulate text, copy or append externally (e.g., StringBuilder).

---

## AgentTextDelta

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents a single incremental assistant-text token (a text "delta") that clients append to reconstruct the current assistant message. Reach for this type when handling streamed or incremental output from an agent instead of receiving a complete message in one piece.

## Remarks
This record models streamed output as small contiguous text fragments. It derives from AgentEvent so the pipeline can treat text deltas uniformly with other event types while keeping the payload minimal. Because it is a record, instances are compared by value (the Delta property), and the type is sealed to prevent extension.

## Example
```csharp
// Collect deltas and build the full message
var deltas = new[]
{
    new AgentTextDelta("Hello"),
    new AgentTextDelta(", world"),
    new AgentTextDelta("!")
};

var builder = new System.Text.StringBuilder();
foreach (var d in deltas)
{
    builder.Append(d.Delta);
}

string fullMessage = builder.ToString(); // "Hello, world!"

// Common pattern when processing an event stream
await foreach (var ev in agentEventStream)
{
    if (ev is AgentTextDelta textDelta)
    {
        builder.Append(textDelta.Delta);
    }
}
```

## Notes
- Delta is typically a partial fragment and may be empty; do not treat a single delta as the complete assistant response.
- Correct reconstruction requires preserving the original order of deltas; consumers must concatenate in sequence.
- If deltas may arrive from multiple threads, synchronize appends — the record itself is immutable but building the final string is not thread-safe.

---

## AgentToolCall

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an immutable event emitted when the assistant requests an external tool. Use this record when producing, handling, logging, or replaying agent activity that involves invoking a tool so consumers can correlate the tool invocation with the originating assistant message and examine the tool name and serialized arguments.

## Remarks
This sealed record is a small, purpose-built subclass of AgentEvent used to record a single tool invocation. It carries the originating message's identifier (MessageId), an opaque ToolCallId for correlating or deduplicating calls, the tool's Name, and the raw ArgumentsJson payload. Keeping ArgumentsJson as a string defers parsing/validation to the consumer, which keeps the event lightweight and decouples the event model from any particular argument schema.

## Example
```csharp
// Create a new tool-call event referencing the assistant message and a JSON argument payload
var evt = new AgentToolCall(
    messageId: assistantMessageId,
    toolCallId: Guid.NewGuid().ToString(),
    name: "web_search",
    argumentsJson: "{ \"query\": \"latest weather\" }"
);

// Pattern-match when processing an AgentEvent stream
if (evt is AgentToolCall call)
{
    Console.WriteLine($"Tool invoked: {call.Name}, args: {call.ArgumentsJson}");
}
```

## Notes
- ArgumentsJson is stored as a raw JSON string and is not validated by the record; consumers must parse and validate it according to the tool's expected schema.
- ToolCallId is opaque (string) and intended for correlation/deduplication; its format is not enforced by this type.
- MessageId refers to the persisted assistant message that triggered the tool call and should be used to trace the event back to the conversation context.

---

## AgentToolResult

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Represents an event emitted when a tool invocation completes and the tool's observation has been persisted as a separate Message. Carries the persisted message's identifier (MessageId), the original tool call identifier (ToolCallId), and the tool output text (Content). Use this event to notify other components that a tool run finished and to provide the reference needed to load the full persisted observation.

## Remarks
This sealed record is an AgentEvent variant intended for event streams or handlers that react to tool results. By carrying the persisted MessageId rather than embedding the entire message object, consumers can lazily fetch or inspect the persisted message from storage, keeping the event lightweight and avoiding duplication of stored data. The record's positional properties provide value-based equality useful for testing and deduplication in event processing.

## Example
```csharp
// Create an event after persisting the tool's observation
var persistedMessageId = Guid.NewGuid(); // returned by the message store
var toolResultEvent = new AgentToolResult(persistedMessageId, "tool-call-123", "Detected 3 objects: ...");

// Pattern-match or inspect the event in an event handler
if (toolResultEvent is AgentToolResult({ ToolCallId: var id, MessageId: var msgId }))
{
    // Use msgId to fetch the persisted message from storage
    // and use id to correlate to the original tool invocation
}
```

## Notes
- MessageId is a Guid that must match the identifier returned by the message persistence layer; consumers typically use it to retrieve the stored Message.
- Content is a string (non-nullable) and may be large; consider streaming or retrieving the persisted message when large payloads are expected.
- As a sealed record, instances are immutable and compare by value; inheritance is not supported.

---

## AgentUserMessagePersisted

> **File:** `src/api/Gabriel.Engine/Services/AgentEvent.cs`  
> **Kind:** record

Emitted as the first event of any turn that originated from a user message (when the server handled a RunAsync request). Carries the authoritative database Guid for the user message that the server just persisted so a client can replace its temporary client-side message id with the real persistent id without needing an extra "GET conversation" round-trip.

## Remarks
This record is a lightweight metadata event used to synchronize client state with the server's persisted data. It is sent only for turns that began from a user message processing path (RunAsync) and is the first event in that turn's stream; it is not emitted for RegenerateAsync flows. Clients typically use this to swap a locally-generated temporary id (e.g. "tmp-xxxxx") for the provided MessageId so references, reactions, and UI state remain consistent after persistence.

## Example
```csharp
// Example event handler sketch — replace tmpLocalId with whatever local tracking you use.
void OnAgentEvent(AgentEvent ev, string tmpLocalId)
{
    if (ev is AgentUserMessagePersisted persisted)
    {
        // Replace the temporary client-side id with the persisted database id
        // in the client's message store so no follow-up GET is required.
        UpdateLocalMessageId(tmpLocalId, persisted.MessageId);
    }
}
```

## Notes
- This event contains only the server-assigned MessageId; clients must correlate it with their own temporary id (which they should record when sending the message).
- Emitted only once per turn and only for RunAsync-originated turns — do not rely on receiving it for regenerate operations.
- If the client fails to track the temporary id, it cannot automatically map the persisted id and will need a separate fetch to reconcile state.

---
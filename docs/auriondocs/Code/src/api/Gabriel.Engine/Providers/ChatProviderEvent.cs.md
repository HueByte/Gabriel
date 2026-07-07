# ChatProviderEvent.cs

> **Source:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`

## Contents

- [ChatProviderEvent](#chatproviderevent)
- [FinishEvent](#finishevent)
- [ReasoningDeltaEvent](#reasoningdeltaevent)
- [TextDeltaEvent](#textdeltaevent)
- [ToolCallReadyEvent](#toolcallreadyevent)
- [FinishReason](#finishreason)

---

## ChatProviderEvent
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

```csharp
public abstract record ChatProviderEvent
```


ChatProviderEvent is the abstract base record for streaming events produced by IChatProvider.StreamAsync. The provider buffers partial tool-call deltas internally and only emits a fully assembled ToolCallReady event, so consumers receive cohesive, fully formed messages rather than partial JSON fragments. As an abstract record, ChatProviderEvent cannot be instantiated directly and serves as the common root for concrete, streaming event types produced during chat sessions.

## Remarks
ChatProviderEvent provides a stable envelope for the chat-streaming events. By exposing all events through a single base type, the streaming pipeline can evolve by adding new derived event shapes without changing consumer code that iterates over `IAsyncEnumerable<ChatProviderEvent>`. It also encapsulates the buffering and reassembly policy inside the provider, keeping the consumer logic focused on handling completed events.

## Notes
- This type is abstract; you cannot instantiate ChatProviderEvent directly. Use a derived event type at runtime (e.g., ToolCallReady) when handling events.
- When processing the stream, prefer pattern matching against concrete derived event types rather than inspecting a generic interface, as new event shapes may be introduced.

---

## FinishEvent
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

```csharp
public sealed record FinishEvent(FinishReason Reason) : ChatProviderEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Reason` | `FinishReason` | — |


FinishEvent is a terminal event in the chat provider's event stream. It signals that the provider has nothing more to emit for the current turn and carries a FinishReason describing why emission has ended. As a sealed record, it is immutable and supports value-based equality, and it derives from ChatProviderEvent to participate in the shared event-handling pipeline.

## Remarks

FinishEvent serves as the canonical end-of-turn marker in the provider’s event stream. By representing termination as its own event type, downstream components can treat completion uniformly alongside other events, simplifying lifecycle management and error handling. The FinishReason communicates the rationale for termination, enabling callers to distinguish between normal completion, a soft stop, or an error condition, without inspecting internal provider state. Because the type is sealed, its termination contract is preserved and not extended by accidental subtypes.

## Notes

- FinishEvent is a reference-type record with value-based equality; instances compare equal if their Reason matches.
- It is immutable; Reason is set at construction and cannot be changed.
- Treat FinishEvent as a clean terminal signal; ensure downstream handlers implement finish logic for all FinishReason values.

---

## ReasoningDeltaEvent
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

```csharp
public sealed record ReasoningDeltaEvent(string Delta) : ChatProviderEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Delta` | `string` | — |


ReasoningDeltaEvent is a sealed record that carries a single string Delta and represents an incremental token of the model's reasoning produced when a streaming reasoning channel is available. It is emitted as part of the ChatProviderEvent stream separate from the final assistant answer. Consumers should treat the Delta as internal chain-of-thought content that can be appended to a live thinking log or debug trace, while the final answer is constructed from other events and persisted separately. Use this symbol when your UI or tooling needs real-time visibility into the model's reasoning steps; if you do not require streaming insight or must avoid exposing thinking tokens to users, you can ignore these events.

## Remarks
This event abstraction decouples streaming reasoning from the final content, enabling clients to subscribe to a dedicated thinking channel without polluting the final answer payload. It supports transparency, debugging, or advanced user interfaces that display the model's evolving thoughts. Remember that this content can reveal internal reasoning, so treat it as sensitive in production scenarios and guard its exposure accordingly.

## Notes
- Delta tokens may arrive in multiple chunks; design consumers to append rather than replace and initialize with a clean accumulator per session.
- Do not display reasoning tokens to end users by default; gate exposure to comply with privacy and policy constraints.
- If your provider does not emit a reasoning channel, this event type will not be produced.

---

## TextDeltaEvent
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

```csharp
public sealed record TextDeltaEvent(string Delta) : ChatProviderEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Delta` | `string` | — |


TextDeltaEvent represents a single incremental token of assistant-provided text in a streaming chat workflow. Each instance carries the Delta string and is emitted as part of a sequence of deltas; consumers accumulate these tokens, in arrival order, to reconstruct the full assistant message.

## Remarks
Placed in the stream alongside other TextDeltaEvent instances, this symbol isolates the notion of a fragment of generated text from the provider's overall event payload. It solves the problem of incremental rendering by enabling consumers to start displaying or processing partial results immediately, while subsequent deltas arrive. By inheriting from ChatProviderEvent, it participates in the provider's event-based pipeline and relies on the ordering guarantees of delta emission to produce coherent messages.

## Example
```csharp
// Assemble a complete message from delta tokens
using System.Linq;

var deltas = new TextDeltaEvent[]
{
    new TextDeltaEvent("Hel"),
    new TextDeltaEvent("lo"),
    new TextDeltaEvent(", " ),
    new TextDeltaEvent("world!")
};

string finalMessage = string.Concat(deltas.Select(d => d.Delta));
// finalMessage == "Hello, world!"
```

## Notes
- Ensure deltas are processed in arrival order; out-of-order assembly leads to garbled text.
- TextDeltaEvent is a record; it is immutable by design—do not mutate its Delta after creation.
- If the delta stream ends unexpectedly, the final concatenation may yield an incomplete message.

---

## ToolCallReadyEvent
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

```csharp
public sealed record ToolCallReadyEvent(string Id, string Name, string ArgumentsJson) : ChatProviderEvent
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `Id` | `string` | — |
| [`Name`](ToolBridge/GabrielToolBridge.cs.md) | `string` | — |
| `ArgumentsJson` | `string` | — |


ToolCallReadyEvent is a concrete, immutable signal indicating that a tool invocation has been prepared and is ready to be executed by the tool execution layer. It carries three pieces of data: Id, a unique identifier used to correlate this event with downstream responses; Name, the identifier of the tool to invoke; and ArgumentsJson, a JSON-encoded payload containing the tool's input parameters. As a sealed record that extends ChatProviderEvent, it participates in the chat provider's event stream as the canonical form of a "ready-to-call tool" notification.

## Remarks
ToolCallReadyEvent isolates the readiness state from the actual dispatching logic, enabling clean routing of tool invocations through the chat system. Its immutability and structural equality simplify caching, testing, and reasoning about event streams. The Id links the lifecycle across queuing, dispatch, and response, while Name and ArgumentsJson carry the essential tool identity and payload for execution.

## Example
```csharp
// Example: creating a ready-to-dispatch tool call
var ready = new ToolCallReadyEvent("tool-42", "SpellCheck", "{\"text\":\"Ths is an exmple.\"}");
```

## Notes
- Ensure ArgumentsJson is valid JSON and matches the tool's expected argument schema. Misformatted or unexpected payloads may fail at the execution stage.
- This type is immutable; if you need to alter data, construct a new ToolCallReadyEvent instance with the updated values.

---

## FinishReason
> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** enum

```csharp
public enum FinishReason
{
    Stop,
    ToolCalls,
    Length,
    Error,
}
```


FinishReason encodes why a chat provider finished producing a response. It enables the caller to decide what to do next - stop streaming, initiate a tool-invocation loop, handle a token-length cutoff, or respond to an error - without peeking into the generation details.

## Remarks
This enum provides a concise, language-native signal for flow control in the chat pipeline. It separates the act of producing content from the orchestration logic, allowing callers to react consistently to natural termination (Stop), tool-driven continuation (ToolCalls), length-bound termination (Length), or a provider failure (Error).

## Example
```csharp
switch (finishReason)
{
    case FinishReason.Stop:
        // normal completion, no further action
        break;
    case FinishReason.ToolCalls:
        // trigger tool invocation loop
        break;
    case FinishReason.Length:
        // content length reached; consider trimming or stopping
        break;
    case FinishReason.Error:
        // surface error to caller or retry
        break;
}
```

## Notes
- Exhaustive handling is recommended; missing a case can lead to incomplete control flow in some contexts.
- ToolCalls is a signaling primitive, not a guarantee of successful tool execution.


---
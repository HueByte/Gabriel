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


ChatProviderEvent is the abstract base type for all streaming events produced by IChatProvider.StreamAsync. It represents the set of events that can occur during a streaming session and enables consumers to handle different event shapes via polymorphism. The provider buffers partial tool-call deltas internally and only emits a ToolCallReady event when a call is fully assembled, so downstream code receives complete payloads without reassembling JSON fragments.

## Remarks
The abstraction isolates streaming semantics from the concrete payloads, allowing the provider to optimize buffering and payload shaping without leaking those details to callers. It also enables a clean separation between the transport/assembly layer and the business logic that handles completed tool calls.

## Example
```csharp
// Consumer pattern: handle concrete event types as they arrive
await foreach (ChatProviderEvent e in chatProvider.StreamAsync())
{
    switch (e)
    {
        case ToolCallReady ready:
            // process the completed tool call
            break;
        default:
            // handle or ignore other event types
            break;
    }
}
```

## Notes
- Do not instantiate ChatProviderEvent directly; it is abstract.
- The stream yields only fully assembled events (e.g., ToolCallReady); do not rely on partial fragments.
- Use type-pattern matching to access payloads of derived events.

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


FinishEvent signals that the current provider turn is complete: the provider has nothing more to emit, and the included FinishReason explains why the turn ended. As a sealed record deriving from ChatProviderEvent, it participates in the same event-stream contract as other provider events, but it is the canonical terminal signal developers check for to know when to stop consuming further events for that turn.

## Remarks
By representing the end of a turn as a dedicated type, the system can distinguish between ongoing data and termination. It centralizes end-of-turn behavior in one symbol, enabling consumers to perform cleanup, transition to the next turn, or present a completed response to the user based on the FinishReason. The FinishReason value is intended to be interpreted by the consumer to determine next steps; this abstraction decouples production of messages from termination semantics.

## Notes
- Terminal nature: do not emit further ChatProviderEvent values after receiving FinishEvent within the same turn; subsequent events can be treated as out of band or ignored.
- FinishReason semantics: ensure alignment between producer and consumer; misalignment can lead to incorrect post-turn handling.

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


ReasoningDeltaEvent represents an incremental token from the model's reasoning stream. It is emitted by chat providers that expose a separate thinking channel (e.g., Grok 4, DeepSeek-R1, OpenAI o-series, Anthropic extended-thinking) as a distinct stream from the final assistant answer. The event carries a Delta string containing the latest segment of the model's internal chain-of-thought, distinct from the final assistant answer. Consumers that want to display or log the thinking in real time can subscribe to this event; however, the Delta is internal reasoning data and should not be treated as the final answer or persisted alongside it. Persistence of final content remains separate from the reasoning stream.

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


TextDeltaEvent represents a chunk of incremental text emitted by a streaming chat provider. It carries a Delta string that should be appended to reconstruct the full message as deltas arrive, enabling progressive rendering and processing of partial content.

## Remarks
As a sealed record deriving from ChatProviderEvent, TextDeltaEvent is immutable and participates in the chat provider's event taxonomy. This abstraction enables streaming consumption: you receive small text fragments as they arrive, and you can render or accumulate them progressively without waiting for a final payload. Maintain the arrival order, since the final message is the concatenation of deltas in sequence.

## Example
```csharp
// Example: accumulate deltas to form the complete message
var accumulated = new System.Text.StringBuilder();

void OnEvent(ChatProviderEvent e)
{
    if (e is TextDeltaEvent delta)
        accumulated.Append(delta.Delta);
}

string GetCompleteMessage() => accumulated.ToString();
```

## Notes
- Deltas are incremental and may be fragments; do not assume each Delta ends at a word boundary.
- If deltas arrive from multiple threads, synchronize access to the accumulator; the event sequence preserves order for a single stream, but consumer code may require synchronization when aggregating.


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


Represents a complete, ready-to-execute tool call payload produced by the chat tooling pipeline. It carries three fields—Id, Name, and ArgumentsJson—and serves as a dispatched instruction to the tool runner to execute the named tool with the supplied arguments.

## Remarks
Acts as a transport boundary between preparation and execution. By encapsulating the invocation details in an immutable record, it enables reliable auditing, correlation, and retry strategies across the tool invocation workflow.

## Example
```csharp
var ready = new ToolCallReadyEvent(
    Id: "tool-001",
    Name: "WeatherForecast",
    ArgumentsJson: "{\"location\":\"Seattle\",\"units\":\"metric\"}"
);
```

## Notes
- ArgumentsJson must be valid JSON; otherwise the executor may fail to parse.
- Id should be unique per invocation to avoid misrouting or clobbering concurrent invocations.

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


FinishReason enumerates why a chat provider's generation finished. It signals whether the assistant produced a final response (Stop), requested tool invocations to continue the dialogue (ToolCalls), hit the model’s output length limit (Length), or encountered a provider-side failure (Error).

## Remarks
FinishReason acts as a contract between the generation engine and the orchestration layer, abstracting control flow decisions away from raw text content. It enables clean streaming semantics by distinguishing normal completion from required tool execution, length-induced termination, or error conditions, and guides the caller on how to proceed within the chat pipeline.

## Example
```csharp
switch (finishReason)
{
    case FinishReason.Stop:
        // Normal completion; end of response stream
        break;
    case FinishReason.ToolCalls:
        // Execute required tool invocations and continue streaming
        break;
    case FinishReason.Length:
        // Content reached token limit; consider truncation or requesting more budget
        break;
    case FinishReason.Error:
        // Propagate or handle provider-side failure
        break;
}
```

## Notes
- ToolCalls implies the agent loop will resume after performing the necessary tool invocations.
- Length may indicate truncation; callers might retry with a larger token budget or adjust prompts.

---
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


ChatProviderEvent is the abstract base type for streaming events emitted by IChatProvider.StreamAsync. It represents a single event in the streaming lifecycle, while concrete event types carry the actual payload. The provider buffers partial tool-call deltas internally and only emits ToolCallReady once a call is fully assembled, so consumers do not need to reassemble JSON fragments.

## Remarks

By exposing a single base type, this abstraction enables consumers to handle diverse event shapes via pattern matching against derived types. It decouples event generation from consumption, allowing different IChatProvider implementations to emit their own concrete event kinds while exposing a uniform stream interface. The emphasis on emitting ToolCallReady only when a call is complete reduces the risk of consumers processing partial data and simplifies downstream logic.

## Notes
- ChatProviderEvent is abstract; it cannot be instantiated directly. Derive concrete events to represent specific streaming scenarios.
- Derived events carry the actual payloads; ChatProviderEvent serves as a discriminated union for all possible stream events.

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


FinishEvent is a terminal chat-provider event that signals there is no more data to emit for the current turn. It carries a FinishReason and derives from ChatProviderEvent, enabling consumers to gracefully conclude a provider's emission path for the turn and react to the reason of termination when appropriate.

## Remarks
FinishEvent serves as an explicit end-of-turn signal in the chat provider's event stream. Using a dedicated type simplifies downstream logic, allowing consumers to pattern-match on FinishEvent to finalize UI state or orchestration without inspecting payloads of ordinary events. As a sealed record, FinishEvent is immutable and identity-driven, which helps ensure deterministic behavior in the event pipeline.

## Notes
- FinishEvent is a terminal event; once observed, it indicates the provider will emit nothing more for this turn.
- FinishReason describes why the provider finished; downstream code should rely on this to adjust flow or UI.
- Because FinishEvent is a record type, two instances with the same Reason compare as equal.

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


ReasoningDeltaEvent is a specialized event that carries an incremental reasoning token emitted by chat providers that expose a separate thinking stream. It wraps a single Delta string and derives from ChatProviderEvent, enabling clients to handle model thoughts distinctly from the final assistant reply. Use this when a provider supports reasoning streaming and you want to surface or log the model's intermediate steps in real time; if no reasoning channel is available, this event will not be emitted, and you should rely on the final message instead.

## Remarks
ReasoningDeltaEvent exists to separate the model's ongoing thinking stream from its final reply. It helps UI layers and debugging tools surface intermediate tokens without conflating them with the polished output. Being a concrete subclass of ChatProviderEvent, it fits neatly into the provider event ecosystem while remaining optional for providers that don't emit reasoning data.

## Notes
- Streaming tokens may arrive incrementally and represent partial thoughts; do not treat them as a complete transcript or a guaranteed representation of the model's final reasoning.
- Do not persist or display ReasoningDeltaEvent content as part of the final assistant transcript; keep it separate from the final output data.


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


TextDeltaEvent is an immutable record that carries a single incremental piece of assistant text as it arrives in a streaming chat flow. Use it when chat output is delivered in chunks (deltas) rather than as a single complete string, so a consumer can append each Delta to build the full message.

## Remarks
TextDeltaEvent participates in the ChatProviderEvent hierarchy and represents a granular unit of streamed text. Making it a sealed record preserves a stable, non-derivable signal for downstream consumers and enables safe pass-through, serialization, or queuing of discrete text fragments. Consumers typically accumulate deltas in order to present the complete message to the user while handling delivery asynchronously.

## Notes
- Delta is immutable; do not mutate Delta after construction.
- The final message is formed by concatenating deltas in their emission order.
- This type is a ChatProviderEvent subtype; handle it via pattern matching or type checks to process text fragments specifically.

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


ToolCallReadyEvent represents a prepared tool invocation within the chat-provider workflow. It carries the invocation's Id, the tool Name to run, and a JSON payload (ArgumentsJson) with the parameters required by the tool. Being a sealed record, it's immutable and participates in value-based equality, which makes it ideal for routing and deduplication in event pipelines.

## Remarks
ToolCallReadyEvent exists to decouple the decision to run a tool from the mechanics of the chat provider. Handlers can pattern-match on this type to route to the correct tool runner, using Id for correlation and ArgumentsJson for runtime deserialization. Because it inherits from ChatProviderEvent, it fits into a family of provider-related events and can be consumed by generic pipelines alongside other events.

## Notes
- Ensure the ArgumentsJson payload is valid JSON before dispatching to the tool runner.
- The Id should be unique for each invocation to avoid collisions in logs and traces.
- The type is immutable; avoid attempting to modify properties after construction.

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


FinishReason describes the termination condition of a generation cycle signaled by the ChatProviderEvent. It enumerates the four principal causes a provider may report when it finishes processing a request: Stop indicates natural, completed content generation; ToolCalls signals that the agent should perform additional tool invocations and resume the flow; Length means the model hit the token limit and may require continuation or adjustment; Error flags a provider-side failure that requires error handling. Consumers can switch on this value to route execution to the appropriate follow-up action (finalize and return content for Stop, invoke tools for ToolCalls, consider continuation or budget adjustments for Length, or handle/propagate the error for Error).

## Remarks
FinishReason abstracts termination semantics from the caller, allowing the provider to communicate exact next steps without exposing internal state details. It supports both streaming and non-streaming flows by clearly signaling whether more work is required (ToolCalls) or if processing has concluded (Stop). By centralizing termination signals, it helps maintain consistent handling across the chat provider and its consumers.

## Example
```csharp
// Example usage demonstrating common branching on finish reason
void HandleFinish(FinishReason finishReason)
{
    switch (finishReason)
    {
        case FinishReason.Stop:
            // deliver final content to the user
            break;
        case FinishReason.ToolCalls:
            // invoke required tools and resume generation
            break;
        case FinishReason.Length:
            // token limit reached; decide on truncation or continuation
            break;
        case FinishReason.Error:
            // surface or log the error and abort or retry as appropriate
            break;
    }
}
```

## Notes
- Length should trigger a well-defined strategy (e.g., request more tokens, adjust prompt, or gracefully truncate) rather than silent truncation.
- ToolCalls implies additional tool-invocation infrastructure is available; without it, this path cannot be exercised meaningfully.
- The enum represents cross-cutting termination semantics; ensure all layers consuming ChatProviderEvent map to these cases consistently.

---
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
// Streaming events emitted by IChatProvider.StreamAsync. The provider buffers
// partial tool-call deltas internally and only emits ToolCallReady once a call
// is fully assembled, so consumers don't need to reassemble JSON fragments.
public abstract record ChatProviderEvent
```


Base event type for events produced by IChatProvider.StreamAsync. Reach for this abstraction when consuming streaming output from a chat provider — handlers should pattern-match on concrete derived events (for example ToolCallReady) to react to specific occurrences. Providers buffer partial tool-call deltas internally and only emit a complete ToolCallReady event once a call is fully assembled, so consumers do not need to reassemble JSON fragments.

## Remarks
This abstract record serves as the extensible root for all streaming event kinds emitted by chat providers. Using a single base type simplifies downstream consumers: the stream yields a uniform event type while concrete derived records carry event-specific payloads. Because it is a record, derived event types participate in C# value-based equality and are convenient to pattern-match and deconstruct.

## Example
```csharp
// Typical consumption pattern when iterating a provider's stream
await foreach (var evt in chatProvider.StreamAsync(request, cancellationToken))
{
    switch (evt)
    {
        case ToolCallReady toolCall:
            HandleToolCall(toolCall);
            break;
        case SomeOtherEvent other:
            HandleOther(other);
            break;
    }
}
```

## Notes
- Providers buffer and assemble partial tool-call deltas; do not attempt to reassemble JSON fragments yourself — wait for the provider to emit ToolCallReady.
- As a record, derived events use value equality; compare types and contents as needed when handling or testing events.
- The type is abstract: expect domain-specific derived records to carry the actual event data.

---

## FinishEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a terminal event emitted by a chat provider to indicate that it has nothing more to produce for the current turn. The record carries a FinishReason that explains why the provider finished; consumers handle this event when they need to perform end-of-turn cleanup or transition state.

## Remarks
This sealed record is part of the ChatProviderEvent hierarchy and is intended as the final event in a stream of provider events for a single turn. It is an immutable, value-based record containing only the FinishReason, so event handlers should inspect the Reason to decide whether the finish was normal, aborted, errored, etc.

## Example
```csharp
// Emitting a finish event from a provider (reason determined earlier)
FinishReason reason = /* determine finish reason */;
var finishEvent = new FinishEvent(reason);

// Consumer that handles provider events
void HandleEvent(ChatProviderEvent ev)
{
    switch (ev)
    {
        case FinishEvent f:
            Console.WriteLine($"Provider finished: {f.Reason}");
            // perform end-of-turn logic here
            break;

        // handle other ChatProviderEvent cases...
    }
}
```

## Notes
- The record is sealed and immutable; equality is value-based (by the Reason property).
- FinishEvent carries no additional payload beyond the Reason — any follow-up data must be provided through other events or external state.

---

## ReasoningDeltaEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a single incremental "reasoning" token (a partial piece of the model's internal chain-of-thought) emitted by a chat provider. Use this event when you need to observe or display the model's intermediate thinking stream separately from final assistant messages — for example, to show progress, debugging output, or an internal thinking visualization.

## Remarks
This sealed record is a specialized ChatProviderEvent carrying a single Delta string. Providers may emit a sequence of these events while a model is producing its reasoning stream; some providers do not support a separate reasoning channel and will never emit these events. Treat the contents as transient, internal chain-of-thought data: it is useful for debugging or UI feedback but should not be treated as the authoritative final assistant response.

## Example
```csharp
// Creating a ReasoningDeltaEvent
var delta = new ReasoningDeltaEvent("...considering possible answers...");

// Consuming events (example of a simple event handler)
void HandleProviderEvent(ChatProviderEvent ev)
{
    if (ev is ReasoningDeltaEvent rd)
    {
        // Append to a UI element or an internal buffer
        AppendToReasoningDisplay(rd.Delta);
    }
    else
    {
        // handle other event kinds
    }
}
```

## Notes
- Delta values are incremental and fragmentary; consumers should concatenate or render them in arrival order rather than treating each as a complete thought.
- Reasoning output may contain sensitive or internal deliberation; avoid persistently logging or exposing it to end users without review or consent.
- Not all providers emit reasoning events — code should tolerate their absence and not rely on them for producing the final assistant answer.

---

## TextDeltaEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a single incremental piece of assistant-generated text (a token or chunk) emitted by a chat provider during a streaming response. Use this event when you handle streaming/partial assistant output — consumers should concatenate successive TextDeltaEvent.Delta values (in arrival order) to reconstruct the assistant's full message.

## Remarks
TextDeltaEvent is a lightweight, immutable record derived from ChatProviderEvent that models streaming assistant content. Many of these events typically arrive for a single assistant turn; they are intentionally small and may contain partial tokens, whitespace, or punctuation. The design keeps token delivery separate from finalization events so consumers can display or process output progressively while awaiting completion.

## Example
```csharp
// Concatenate delta events to build the assistant message
var builder = new System.Text.StringBuilder();
foreach (var ev in incomingEvents)
{
    if (ev is TextDeltaEvent delta)
    {
        builder.Append(delta.Delta);
    }
    else if (ev is /* some finalization event */)
    {
        // turn complete
        var fullMessage = builder.ToString();
        // handle full message...
        builder.Clear();
    }
}
```

## Notes
- Maintain the original arrival order when concatenating deltas — reordering will corrupt the reconstructed text.
- A single TextDeltaEvent does not represent a complete assistant message; treat it as a fragment (it may be empty or contain only a partial token).


---

## ToolCallReadyEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a notification emitted when a prepared tool invocation is ready to be executed. It carries a unique call Id, the tool Name, and the tool arguments serialized as a JSON string; consume this event when a chat provider or orchestrator needs to hand off or schedule the actual tool execution.

## Remarks
This is a small immutable DTO (record) that inherits from ChatProviderEvent and is intended as a lightweight, transport-friendly message between components. Arguments are kept as a raw JSON string to avoid coupling the event type to any particular argument schema or deserialization strategy; consumers can parse the JSON into a concrete argument type when they are ready to execute the tool.

## Example
```csharp
using System.Text.Json;

// Create the event when a tool call has been prepared
var evt = new ToolCallReadyEvent(
    Id: "call-123",
    Name: "translate",
    ArgumentsJson: "{ \"text\": \"Hello\", \"targetLang\": \"es\" }"
);

// Consumer: parse the arguments when executing the tool
var argsDoc = JsonDocument.Parse(evt.ArgumentsJson);
string text = argsDoc.RootElement.GetProperty("text").GetString();
string targetLang = argsDoc.RootElement.GetProperty("targetLang").GetString();

// Or deserialize into a typed DTO
var typedArgs = JsonSerializer.Deserialize<TranslateArgs>(evt.ArgumentsJson);
```

## Notes
- ArgumentsJson is stored as a raw JSON string and is not validated by this record; parsing or deserialization must be performed by the consumer and may throw on invalid input.
- The record is immutable; use the `with` expression to create a modified copy if needed.
- Id and Name are descriptive identifiers but this type does not enforce uniqueness or non-empty values.

---

## FinishReason

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** enum

Represents the reason why an assistant/chat response finished. Use this enum when handling provider or chat events to decide next steps (for example: finalize output, invoke tools and continue streaming, attempt a continuation after a length cutoff, or surface a provider error).

## Remarks
This enum is intended for consumers of chat/provider events (agent loop, UI, orchestration code) to drive follow-up actions. In particular, ToolCalls signals that the assistant expects external tool invocation and that the agent loop should execute those tools and then resume/re-stream the assistant output; Length indicates the model stopped because of token/output limits and may require an explicit continuation request; Error denotes a provider-side failure rather than a normal conversational termination.

## Example
```csharp
switch (finishReason)
{
    case FinishReason.Stop:
        FinalizeResponse(content);
        break;
    case FinishReason.ToolCalls:
        await ExecuteToolsAndResumeAsync(toolRequests);
        break;
    case FinishReason.Length:
        await RequestContinuationAsync(partialContent);
        break;
    case FinishReason.Error:
        HandleProviderError(errorInfo);
        break;
}
```

## Notes
- ToolCalls typically requires parsing the assistant's tool-call payload and running the requested tools before resuming; failing to do so will break expected agent behavior.
- Length means the returned content may be truncated — preserve conversation/context when requesting continuation.
- Error indicates a provider-level problem (network, model failure, etc.); treat differently from user-level or domain errors.
- This is a simple discriminant enum (not a flags/bitmask type).

---
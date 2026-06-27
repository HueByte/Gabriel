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

Represents a streaming event emitted by IChatProvider.StreamAsync. Use this abstract base when handling events produced by a chat provider; consumers typically switch or pattern-match on concrete event variants (for example, a ToolCallReady event) rather than reassembling partial fragments themselves.

## Remarks
This abstract record centralizes the different event kinds that a chat provider can emit during streaming. Providers buffer partial tool-call deltas internally and only emit a completed ToolCallReady event once a call is fully assembled, so consumers do not need to reconstruct JSON fragments or handle low-level buffering.

## Example
```csharp
// Typical event loop that handles different event kinds
await foreach (var evt in chatProvider.StreamAsync(request, cancellationToken))
{
    switch (evt)
    {
        case ToolCallReady toolCall:
            HandleToolCall(toolCall);
            break;
        default:
            HandleOtherEvent(evt);
            break;
    }
}
```

## Notes
- Providers hide partial tool-call fragments: do not expect to receive JSON delta pieces for tool calls — you'll get a fully assembled ToolCallReady instead.
- This type is a record (value semantics) and should be treated as immutable when handling events.

---

## FinishEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

A terminal ChatProviderEvent that indicates the provider has nothing more to emit for the current turn. It carries a FinishReason describing why the provider finished; consumers handle this event to stop streaming or to finalize turn-level state.

## Remarks
This record represents the logical end of a single turn's output from a chat provider. It decouples the concept of "no more data for this turn" from transport- or provider-specific details by carrying a FinishReason. Being a sealed positional record, it is immutable and uses value-based equality, making it suitable for event comparison and pattern matching.

## Example
```csharp
// receive an event from a provider and handle terminal condition
ChatProviderEvent evt = await provider.NextEventAsync();
if (evt is FinishEvent finish)
{
    // stop streaming tokens for this turn and inspect why the provider finished
    StopStreaming();
    var reason = finish.Reason;
    LogFinishReason(reason);
}
```

## Notes
- FinishEvent signals the end of output for the current turn only; a provider may emit events for later turns.
- As a sealed record, instances are immutable and compared by value; use pattern matching or property access to obtain the Reason.

---

## ReasoningDeltaEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a single incremental "reasoning" token emitted by a chat provider as part of a separate thinking stream. Use this event when you need to observe, display, or persist the model's internal chain-of-thought as it is produced; it is not the final assistant message and should be treated as an implementation detail of the provider.

## Remarks
This record exposes small, ordered fragments of a model's internal reasoning (the "thinking" stream) via its Delta property. The abstraction exists so providers can surface reasoning separately from user-facing assistant content — useful for debugging, developer tooling, or UIs that display intermediate thought — while keeping final responses and reasoning distinct. Not all providers emit these events; consumers should handle their absence gracefully and should not treat reasoning deltas as authoritative final output.

## Example
```csharp
// simple handler that appends reasoning deltas to a buffer
var reasoningBuilder = new StringBuilder();

void HandleEvent(ChatProviderEvent evt)
{
    if (evt is ReasoningDeltaEvent r)
    {
        // append incremental reasoning as it arrives
        reasoningBuilder.Append(r.Delta);
        // update a developer-only UI element, log, or persist for debugging
        Console.WriteLine("[Reasoning Delta] " + r.Delta);
    }
}
```

## Notes
- Delta values are incremental fragments and may need to be concatenated in arrival order to reconstruct the full reasoning stream.
- Providers may emit none of these events; code should not assume their presence.
- Treat reasoning deltas as internal instrumentation: avoid exposing them directly to end users or relying on them for authoritative answers.
- The record is immutable (sealed record) and intended for simple transport of text fragments.

---

## TextDeltaEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents a single incremental piece (token/chunk) of assistant-produced text emitted by a chat provider. Consumers receive zero or more of these events during a streaming response and should concatenate the Delta values, in arrival order, to reconstruct the final assistant message.

## Remarks
This record is the minimal event type used for streaming/partial assistant output and extends ChatProviderEvent. It exists so providers can deliver output progressively (reducing latency and memory pressure) while consumers can reassemble the full text by joining the ordered deltas. The record is immutable and contains exactly one string property (Delta) that holds the text fragment.

## Example
```csharp
// Reconstruct full assistant message from a sequence of TextDeltaEvent
var deltas = new[] {
    new TextDeltaEvent("Hello"),
    new TextDeltaEvent(", wor"),
    new TextDeltaEvent("ld!")
};
var full = string.Concat(deltas.Select(e => e.Delta));
// full == "Hello, world!"
```

## Notes
- Preserve the original event order when concatenating; ordering determines the final message.
- A Delta may be an empty string or a partial token; do not assume each event is a complete sentence.
- The record is immutable (sealed) — consumers should not expect mutable state on the event.

---

## ToolCallReadyEvent

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** record

Represents an event emitted by a chat provider to indicate that a tool call has been prepared and is ready to be executed. It carries the tool's identifier, a human-readable name, and the tool call arguments serialized as a JSON string — use this when handing off a planned tool invocation from the chat provider to a tool dispatcher or executor.

## Remarks
This sealed record is a lightweight, immutable value object intended for event-based handoffs between components: chat providers create instances when they decide a tool should be invoked, and execution components deserialize ArgumentsJson to perform the call. Keeping the arguments as a JSON string avoids binding the event to a specific argument type or schema, but requires consumers to validate or deserialize the payload according to the expected tool contract.

## Example
```csharp
using System.Text.Json;
using System.Collections.Generic;

var evt = new ToolCallReadyEvent(
    Id: "tool-123",
    Name: "Search",
    ArgumentsJson: "{\"query\":\"hello world\",\"limit\":10}"
);

// Deserialize into a flexible shape for inspection
var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(evt.ArgumentsJson);
string query = args["query"].GetString();
int limit = args["limit"].GetInt32();

// Now dispatch to the appropriate tool using Id or Name
// dispatcher.Invoke(evt.Id, args);
```

## Notes
- ArgumentsJson must be valid JSON; consumers should handle parse errors and validate the argument schema before executing the tool.
- Records are immutable and compare by value — equality checks compare the three properties rather than object references.
- Do not assume a fixed schema for ArgumentsJson; different tools may expect different shapes, so prefer explicit validation or typed deserialization when the target tool's schema is known.

---

## FinishReason

> **File:** `src/api/Gabriel.Engine/Providers/ChatProviderEvent.cs`  
> **Kind:** enum

Indicates why a chat/assistant response stream stopped. Use this enum when handling provider events or streaming responses so the client (or agent loop) can decide whether to stop, run tool calls and continue streaming, retry/extend after truncation, or surface an error.

## Remarks
This enum is produced by chat providers to classify the terminal condition of a response stream. Consumers use it to determine control flow: Stop means the assistant completed its reply; ToolCalls signals the assistant is requesting external tool execution (the agent loop should perform those calls and re-stream); Length indicates the model hit an output limit and the response may be truncated; Error represents a provider-side failure.

## Example
```csharp
switch (finishReason)
{
    case FinishReason.Stop:
        // Final content received — render to user
        break;
    case FinishReason.ToolCalls:
        // Execute requested tools, then continue the agent loop / re-stream
        break;
    case FinishReason.Length:
        // Response was truncated — consider requesting continuation or increasing limits
        break;
    case FinishReason.Error:
        // Surface provider error to caller or retry
        break;
}
```

## Notes
- Treat ToolCalls as a signal to run external tools and continue the conversation rather than a final answer.
- Length means the output was cut off by model/token limits; don't assume content is complete.
- When receiving unknown/added enum values (from newer providers), handle them defensively (e.g., treat as Error or log and abort) to preserve forward compatibility.

---
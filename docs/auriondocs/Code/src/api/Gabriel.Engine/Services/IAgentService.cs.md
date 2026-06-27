# IAgentService

> **File:** `src/api/Gabriel.Engine/Services/IAgentService.cs`  
> **Kind:** interface

Runs the agent (ReAct) loop for a single user turn, exposes the same streaming model for regenerations, and provides a snapshot of context-window metrics. Use this interface when you need to start or re-run an assistant turn and consume its emitted AgentEvent stream (for streaming SSE/websocket responses, UI updates, or incremental persistence).

## Remarks
IAgentService surfaces agent activity as an `IAsyncEnumerable<AgentEvent>` so callers can process events incrementally as they are produced. Input validation and quick failures (for example: empty input, missing conversation, or invalid regenerate requests) are thrown synchronously to enable the HTTP layer to return an appropriate status before streaming begins; errors that occur while the agent is running are emitted as AgentError events in the returned stream. RegenerateAsync produces a new assistant variant while marking the previous variant inactive and preserving a shared VariantGroupId so the UI can present alternative completions.

## Example
```csharp
// Start a new agent run and stream events to the caller
async Task StreamAssistantReply(IAgentService agentService, Guid conversationId, string userInput, CancellationToken ct)
{
    var events = await agentService.RunAsync(conversationId, userInput, ct);

    await foreach (var e in events.WithCancellation(ct))
    {
        switch (e)
        {
            case AgentEvent.TextChunk text:
                // render incremental assistant text
                Console.Write(text.Content);
                break;
            case AgentEvent.AgentError err:
                // surface failure to user / log
                Console.Error.WriteLine(err.Message);
                break;
            case AgentEvent.ToolCall tool:
                // show tool invocation / results
                break;
            // handle other AgentEvent variants as needed
        }
    }
}

// Regenerate an assistant reply and stream the new variant
async Task RegenerateReply(IAgentService agentService, Guid conversationId, Guid assistantMessageId, CancellationToken ct)
{
    var events = await agentService.RegenerateAsync(conversationId, assistantMessageId, ct);
    await foreach (var e in events.WithCancellation(ct))
    {
        // handle streamed events as above
    }
}
```

## Notes
- Pre-flight validation failures are thrown synchronously (before any streaming begins); handle these exceptions to return proper HTTP status codes rather than relying on the event stream.
- Consumers must enumerate the returned IAsyncEnumerable to receive events; the stream may be long-lived and should be consumed with a CancellationToken to abort cleanly.
- RegenerateAsync will throw synchronously for not-found, non-assistant messages, or already-inactive variants — those checks occur before streaming a new variant.
- Do not assume the entire reply is available up-front; treat the sequence as incremental output that can include both content chunks and structured events (tool calls, errors, finish signals).
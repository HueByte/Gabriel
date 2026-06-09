# IAgentService

> **File:** `src/api/Gabriel.Engine/Services/IAgentService.cs`  
> **Kind:** interface

Runs the agent (ReAct) orchestration for a conversation turn, allows regenerating a previously produced assistant reply, and exposes a snapshot of context-window metrics. Use RunAsync when you need a streaming sequence of AgentEvent values for a single user turn (for example to stream tokens or intermediate tool results to a client), RegenerateAsync when you want to produce a new variant for an existing assistant message, and GetContextMetricsAsync when the UI or service needs the current context-window sizing/usage for compaction or progress indicators.

## Remarks
IAgentService centralizes the high-level agent lifecycle used by the API layer and UI. It intentionally returns `IAsyncEnumerable<AgentEvent>` for RunAsync and RegenerateAsync so callers can stream events (token deltas, tool invocations, final results, or AgentError events) as they occur instead of waiting for a finished payload. The interface separates "pre-flight" validation (synchronous exceptions for missing conversation, invalid inputs, not-found conditions, or inappropriate message types) from in-stream failures (which are emitted as AgentError events), enabling the HTTP layer to choose appropriate status codes before streaming begins.

## Example
```csharp
// Stream an agent turn to the client
await foreach (var evt in agentService.RunAsync(conversationId, userInput, ct))
{
    switch (evt)
    {
        case AgentEvent.Token token:    /* send token to client */ break;
        case AgentEvent.Final result:   /* handle final assistant reply */ break;
        case AgentEvent.AgentError err: /* surface error to client UI */ break;
    }
}

// Regenerate an assistant message variant
await foreach (var evt in agentService.RegenerateAsync(conversationId, assistantMessageId, ct))
{
    // same event consumption model as RunAsync
}

// Read context metrics
var metrics = await agentService.GetContextMetricsAsync(conversationId, ct);
// metrics can be used to render a compaction progress bar or decide whether to call compaction APIs
```

## Notes
- Pre-flight validation throws synchronously: callers should catch these exceptions to return appropriate HTTP statuses before beginning any streaming/SSE response.
- In-stream failures are emitted as AgentError events rather than thrown; consumers must handle AgentError when enumerating the returned IAsyncEnumerable.
- RegenerateAsync marks the existing variant inactive and produces a new variant that shares the same VariantGroupId — UIs should refresh message state and present variant choices to users after regeneration completes.
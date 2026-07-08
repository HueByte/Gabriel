# IAgentService

> **File:** `src/api/Gabriel.Engine/Services/IAgentService.cs`  
> **Kind:** interface

```csharp
public interface IAgentService
```


IAgentService is an abstraction over the per-turn agent workflow for a chat conversation. It exposes three asynchronous operations: RunAsync drives the ReAct loop for a single user turn and yields a stream of AgentEvent events, RegenerateAsync creates an alternate assistant reply for an existing message, and GetContextMetricsAsync returns a snapshot of the current context-window state for progress awareness. Callers receive a streaming `IAsyncEnumerable<AgentEvent>` from RunAsync, with synchronous pre-flight validation that may throw to allow the API layer to respond appropriately before streaming begins; any in-stream failures are surfaced as AgentEvent items.

## Remarks
IAgentService centralizes the orchestration of a single-turn interaction, decoupling the API surface from concrete implementations and enabling testability via the interface. It coordinates a conversation via conversationId, processes the user's input, and supports cancellation through CancellationToken. RegenerateAsync ties into the same variant-tracking flow used by the UI to present alternative assistant replies, while GetContextMetricsAsync feeds UI components with context-usage awareness so users can gauge how close they are to configured thresholds. The combination of real-time AgentEvent streaming and contextual metrics enables responsive applications that reflect both progress and results as they evolve.

## Example
```csharp
// Example: driving a single-turn interaction and inspecting metrics
async Task DemoAsync(IAgentService service, Guid conversationId, string userInput, CancellationToken ct)
{
    await foreach (var evt in service.RunAsync(conversationId, userInput, ct))
    {
        // handle AgentEvent (real-time updates, streaming results, or errors)
    }

    ContextMetrics metrics = await service.GetContextMetricsAsync(conversationId, ct);
    // use metrics to render a progress indicator
}
```

## Notes
- RunAsync and RegenerateAsync perform pre-flight validation and may throw synchronously for common bad inputs (e.g., empty input, missing conversation, not-found or inappropriate variant). Ensure callers map these exceptions to appropriate HTTP responses before streaming starts.

# IAgentService

> **File:** `src/api/Gabriel.Engine/Services/IAgentService.cs`  
> **Kind:** interface

```csharp
public interface IAgentService
```


IAgentService is an asynchronous orchestration surface used to drive the ReAct-style interaction loop for a single conversation turn. It exposes a streaming RunAsync flow that yields AgentEvent values as the agent reasoner progresses, a RegenerateAsync operation to produce alternate assistant replies, and a GetContextMetricsAsync call to snapshot the current context window for UI rendering.

## Remarks
By separating orchestration from API concerns and delivering events as they occur, this interface enables responsive user interfaces and robust streaming behavior. RunAsync performs pre-flight validation (for example empty input or a missing conversation) and may throw synchronously so the API layer can return the appropriate status before SSE headers are sent; in-stream failures are surfaced as AgentError events. RegenerateAsync marks the existing variant inactive and produces a new variant within the same VariantGroupId, allowing the UI to present a chooser between alternatives. GetContextMetricsAsync returns a ContextMetrics snapshot that UI components can use to render progress indicators and decide when to fetch additional context.

## Example
```csharp
// Streaming a single user turn
await foreach (var e in agentService.RunAsync(conversationId, userInput, ct))
{
    RenderEvent(e);
}
```

```csharp
// Retrieve current context metrics for UI progress
var metrics = await agentService.GetContextMetricsAsync(conversationId, ct);
```

## Notes
- RunAsync pre-flight validation is synchronous to allow the API layer to respond with the correct HTTP status before streaming begins.
- In-stream failures are delivered as AgentError events rather than exceptions.
- RegenerateAsync requires a valid, active assistant message and shares its VariantGroupId with other variants to facilitate user selection.
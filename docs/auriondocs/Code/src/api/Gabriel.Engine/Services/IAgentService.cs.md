# IAgentService

> **File:** `src/api/Gabriel.Engine/Services/IAgentService.cs`  
> **Kind:** interface

```csharp
public interface IAgentService
```


IAgentService is the per-conversation agent orchestration contract. Use RunAsync to kick off a single-turn ReAct loop and stream AgentEvent updates (including error events), RegenerateAsync to produce an alternative assistant reply for a given message, and GetContextMetricsAsync to snapshot the context-window state for progress indication.

## Remarks
IAgentService abstracts the lifecycle of agent interactions for a single conversation, encapsulating streaming events, reply regeneration, and context visibility. It hides the ReAct loop orchestration behind a clean, asynchronous API surface that API layers and clients can rely on. RunAsync provides a real-time stream of AgentEvent observations that UI components can render, while RegenerateAsync creates a new variant for the same conversational context without mutating the original, enabling exploration of alternatives. GetContextMetricsAsync surfaces metrics that help the UI decide when to display progress indicators or adjust how much history to fetch.

## Notes
- Pre-flight validation exceptions are thrown synchronously so API layers can map to appropriate HTTP status codes before streaming begins.
- RegenerateAsync enforces that the target message exists, belongs to the assistant, and is active; invalid states throw synchronously.
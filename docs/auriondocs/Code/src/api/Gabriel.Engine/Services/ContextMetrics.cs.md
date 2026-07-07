# ContextMetrics

> **File:** `src/api/Gabriel.Engine/Services/ContextMetrics.cs`  
> **Kind:** record

```csharp
public record ContextMetrics(
    int CurrentTokens,
    int ContextWindowTokens,
    int CompactThresholdTokens,
    double CompactThresholdRatio,
    int MessagesAfterCut,
    bool IsSummarized,
    int SummaryTokens,
    
    int SystemPromptTokens,
    
    int ProjectPromptTokens,
    
    int MemoryTokens,
    
    
    
    int ToolsTokens,
    
    int ConversationTokens)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `CurrentTokens` | `int` | — |
| `ContextWindowTokens` | `int` | — |
| `CompactThresholdTokens` | `int` | — |
| `CompactThresholdRatio` | `double` | — |
| `MessagesAfterCut` | `int` | — |
| `IsSummarized` | `bool` | — |
| `SummaryTokens` | `int` | — |
| `SystemPromptTokens` | `int` | — |
| `ProjectPromptTokens` | `int` | — |
| `MemoryTokens` | `int` | — |
| `ToolsTokens` | `int` | — |
| `ConversationTokens` | `int` | — |


ContextMetrics is a record that captures a snapshot of token usage and decision-related context when preparing the next turn. It is surfaced via IAgentService.GetContextMetricsAsync so the UI can display how much of the current context window is used and whether a summarization or trimming will occur for the upcoming turn. The per-category breakdown fields (SystemPromptTokens through ConversationTokens) mirror exactly what ToProviderHistory assembles, enabling a grid visualization that adds up to CurrentTokens in a way that reflects the backend's actual decision-making.

## Remarks
ContextMetrics provides a stable, serializable snapshot of the contextual baggage considered for the next turn. It decouples the UI from provider internals, letting dashboards display a “context used / until compact” indicator that remains in sync with how the engine will compose or trim messages. It helps diagnose and tune the system by revealing where token usage concentrates (prompts, memories, tools, or ongoing conversation).

## Example
```csharp
// Most common usage: fetch and display current context usage
var metrics = await agentService.GetContextMetricsAsync(...);
Console.WriteLine($"CurrentTokens={metrics.CurrentTokens}, ConversationTokens={metrics.ConversationTokens}, SystemPromptTokens={metrics.SystemPromptTokens}");
```

## Notes
- The per-category buckets are intended to sum into the overall CurrentTokens when the same per-message overhead is accounted for; do not interpret the buckets as exact independent totals.
- CompactThresholdRatio is a double; comparisons should consider floating-point precision and potential rounding differences between components.
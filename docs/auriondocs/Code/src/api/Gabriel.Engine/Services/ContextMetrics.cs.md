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


ContextMetrics is an immutable snapshot that captures how tokens are consumed for the current turn and how that consumption is distributed across contributing sources (system prompts, memories, tools, and the conversation). It is produced by GetContextMetricsAsync and surfaced to the UI so developers can display a faithful indicator of context usage and whether a summarization step was applied.

## Remarks
ContextMetrics acts as a stable contract between backend context evaluation and the frontend visualization. It aggregates category-level token counts to support consistent visual indicators and debugging. The record is immutable, ensuring a single snapshot can be reused for display without downstream mutation.

## Example
```csharp
public async Task ShowMetrics(IAgentService agent, Guid conversationId, CancellationToken ct)
{
    ContextMetrics m = await agent.GetContextMetricsAsync(conversationId, ct);
    Console.WriteLine($"Current: {m.CurrentTokens} tokens; ContextWindow: {m.ContextWindowTokens}; Summarized: {m.IsSummarized} (SummaryTokens={m.SummaryTokens})");
}
```

## Notes
- ContextMetrics is a snapshot; values reflect the moment GetContextMetricsAsync completes and may change with subsequent turns. Do not rely on cross-call invariants beyond the current retrieval.
- To analyze historical provider behavior, consult ToProviderHistory or related diagnostics; ContextMetrics focuses on the current-turn accounting rather than full history.

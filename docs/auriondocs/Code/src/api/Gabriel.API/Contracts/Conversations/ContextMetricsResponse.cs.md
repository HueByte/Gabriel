# ContextMetricsResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs`  
> **Kind:** record

```csharp
public record ContextMetricsResponse(
    
    
    int CurrentTokens,
    
    int ContextWindowTokens,
    
    int CompactThresholdTokens,
    
    
    double CompactThresholdRatio,
    
    int MessagesAfterCut,
    
    
    bool IsSummarized,
    
    
    
    
    
    
    int SystemPromptTokens,
    
    int ProjectPromptTokens,
    
    int MemoryTokens,
    
    int SummaryTokens,
    
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
| `SystemPromptTokens` | `int` | — |
| `ProjectPromptTokens` | `int` | — |
| `MemoryTokens` | `int` | — |
| `SummaryTokens` | `int` | — |
| `ToolsTokens` | `int` | — |
| `ConversationTokens` | `int` | — |


ContextMetricsResponse is a serializable snapshot of the current context usage and summarization state, designed to travel across process or service boundaries without leaking engine-types. Use it when you need to display or reason about token counts, thresholds, and the per-category breakdown that underpin the UI and intelligent summarization decisions.

## Remarks
Decoupling from engine types, ContextMetricsResponse defines a stable wire surface for telemetry and UI consumption while the engine evolves independently. The per-category breakdown (SystemPromptTokens, ProjectPromptTokens, MemoryTokens, SummaryTokens, ToolsTokens, ConversationTokens) makes it easy to show users where token budget is consumed and to verify that CurrentTokens equals the sum of those categories. IsSummarized signals whether any compacted content has been rolled into the conversation.

## Notes
- Per-category token fields are intended to sum to CurrentTokens; consumers should not rely on them for other calculations.
- CompactThresholdRatio should be interpreted as a percentage (e.g., 0.8 means 80%); guard against ContextWindowTokens == 0 to avoid undefined division.
- This DTO is strictly a transport contract; do not rely on it for engine-side logic.
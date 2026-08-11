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


ContextMetricsResponse is a wire-friendly record that carries the engine's contextual metrics for a conversation without exposing engine-layer types. It aggregates the current token counts, the contextual window size, the threshold for triggering compaction, and a per-category breakdown that powers UI indicators and provider history.

## Remarks
This symbol exists to decouple internal ContextMetrics from the public API, enabling stable contracts across internal refactors while still conveying detailed usage statistics to the client. It exposes both global metrics (CurrentTokens, ContextWindowTokens, CompactThresholdTokens, CompactThresholdRatio, MessagesAfterCut, IsSummarized) and a per-category breakdown (SystemPromptTokens, ProjectPromptTokens, MemoryTokens, SummaryTokens, ToolsTokens, ConversationTokens) so consumers can render progress, costs, and summarization state consistently.

## Notes
- CurrentTokens should equal the sum of the per-category token fields (SystemPromptTokens + ProjectPromptTokens + MemoryTokens + SummaryTokens + ToolsTokens + ConversationTokens). If these diverge, the data is inconsistent and should be treated as an error at serialization time.
- CompactThresholdRatio is a double representing a fraction of the ContextWindowTokens (e.g., 0.8 for 80%). When displaying UI, consider formatting to avoid precision artifacts and ensure consistent progress visuals.
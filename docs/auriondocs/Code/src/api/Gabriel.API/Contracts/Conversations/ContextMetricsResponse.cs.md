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


ContextMetricsResponse is a transport-friendly record that mirrors the engine's ContextMetrics for over-the-wire use. It aggregates the current token count, the provider's context window, and a per-category breakdown of tokens (system prompt, project prompt, memories, rolling summaries, tools, and conversation) along with thresholds and summarization state so callers can render UI and make decisions without referencing engine types.

## Remarks
To decouple the API surface from engine internals and preserve stability across internal refactors, this record is kept separate from Gabriel.Engine types. It serves as a stable transport contract that carries granular context accounting, enabling consistent UI and provider behavior without leaking implementation details.

## Example
```csharp
// Concrete usage example
var metrics = new ContextMetricsResponse(
    CurrentTokens: 1280,
    ContextWindowTokens: 256000,
    CompactThresholdTokens: 1024,
    CompactThresholdRatio: 0.8,
    MessagesAfterCut: 5,
    IsSummarized: true,
    SystemPromptTokens: 320,
    ProjectPromptTokens: 128,
    MemoryTokens: 256,
    SummaryTokens: 0,
    ToolsTokens: 20,
    ConversationTokens: 0
);
```

## Notes
- The sum of the per-category tokens (SystemPromptTokens + ProjectPromptTokens + MemoryTokens + SummaryTokens + ToolsTokens + ConversationTokens) should equal CurrentTokens; mismatches indicate unsynced accounting.
- CompactThresholdRatio should reflect the ratio of CompactThresholdTokens to ContextWindowTokens (roughly 0.0–1.0). If out of range, clamp or validate at call-sites.
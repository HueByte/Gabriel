# ContextMetricsResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs`  
> **Kind:** record

A lightweight, API-stable transport record that mirrors the engine-layer ContextMetrics. Use this when returning context/usage information over the wire (for UI display or provider decisions) without exposing internal engine types.

## Remarks
This record exists to decouple the public API surface from the internal Gabriel engine type (ContextMetrics). It provides a compact, serializable snapshot of the conversation context: total token usage, per-category token estimates, the provider context window, and the threshold information used to decide when compaction (MaybeCompactAsync) would run. The per-category fields are the same pieces AgentContext.ToProviderHistory assembles and sum to CurrentTokens, making it convenient for UI grids and progress indicators.

## Example
```csharp
// Constructing a response to send to a client or provider
var response = new ContextMetricsResponse(
    CurrentTokens: 12_345,
    ContextWindowTokens: 256_000,
    CompactThresholdTokens: 204_800,
    CompactThresholdRatio: 0.8,
    MessagesAfterCut: 3,
    IsSummarized: true,
    SystemPromptTokens: 1_200,
    ProjectPromptTokens: 0,
    MemoryTokens: 2_000,
    SummaryTokens: 500,
    ToolsTokens: 100,
    ConversationTokens: 8_345);

// UI: show percent of window used before compacting
double percentBeforeCompact = response.CompactThresholdRatio * 100.0; // 80.0
```

## Notes
- Values are estimated token costs, not exact counts; different tokenizers/models may produce different actual counts.
- Category fields may be zero (for example ProjectPromptTokens or SummaryTokens) when that piece is absent.
- The integer fields are expected to sum to CurrentTokens; callers/reporting should rely on CurrentTokens for provider comparisons rather than recomputing from the parts.
# ContextMetricsResponse

> **File:** `src/api/Gabriel.API/Contracts/Conversations/ContextMetricsResponse.cs`  
> **Kind:** record

Represents token-count metrics for a conversation context sent over the API. This transport-friendly record mirrors the engine-layer ContextMetrics so callers (UI, provider integrations) can inspect the total context size, the compaction threshold, and a per-category token breakdown without depending on internal engine types.

## Remarks
Kept deliberately separate from the engine's ContextMetrics to avoid leaking internal types and to keep the public API stable across internal refactors. The CurrentTokens value matches the sum of the per-category fields below and is the value used by MaybeCompactAsync and by providers to decide when compaction or other actions are necessary. The per-category fields are ordered to match the UI legend (top-to-bottom) and represent estimated token costs for the pieces assembled by AgentContext.ToProviderHistory.

## Example
```csharp
// Typical construction and return from a controller action
var metrics = new ContextMetricsResponse(
    CurrentTokens: 5120,
    ContextWindowTokens: 6400,
    CompactThresholdTokens: 5120,
    CompactThresholdRatio: 0.8,
    MessagesAfterCut: 3,
    IsSummarized: true,
    SystemPromptTokens: 500,
    ProjectPromptTokens: 0,
    MemoryTokens: 1200,
    SummaryTokens: 300,
    ToolsTokens: 200,
    ConversationTokens: 2920);

return Ok(metrics);
```

## Notes
- Values are estimates of token cost as assembled for provider calls, not exact per-model token accounting.  
- CurrentTokens should equal the sum of the per-category fields; consumers should treat it as authoritative for threshold checks.  
- CompactThresholdRatio is typically CompactThresholdTokens / ContextWindowTokens; if ContextWindowTokens can be zero, guard against divide-by-zero when recomputing the ratio client-side.
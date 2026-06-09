# ContextMetrics

> **File:** `src/api/Gabriel.Engine/Services/ContextMetrics.cs`  
> **Kind:** record

A snapshot of token usage and compaction state the agent would observe immediately before sending the next turn. Use this record when you need to display or reason about how many tokens are currently consumed, how close the conversation is to the provider's compaction threshold, and which parts of the context (system prompt, project prompt, memories, tools, conversation, rolling summary) contribute to the total.

## Remarks
This structure is produced by the agent-side logic (via IAgentService.GetContextMetricsAsync) so the UI or diagnostics can show a "context used / until compact" indicator that matches the backend's actual compaction decision. The per-category fields mirror how messages and auxiliary content are assembled for providers (see ToProviderHistory), so the individual category totals add up to CurrentTokens modulo the fixed per-message overhead the provider charges. ToolsTokens is included in the same window budget even though tool descriptors are not part of the messages array.

## Example
```csharp
// Create a snapshot (most callers will receive this from IAgentService.GetContextMetricsAsync)
var metrics = new ContextMetrics(
    CurrentTokens: 3142,
    ContextWindowTokens: 8192,
    CompactThresholdTokens: 6550,
    CompactThresholdRatio: 0.8,
    MessagesAfterCut: 12,
    IsSummarized: true,
    SummaryTokens: 420,
    SystemPromptTokens: 80,
    ProjectPromptTokens: 0,
    MemoryTokens: 250,
    ToolsTokens: 200,
    ConversationTokens: 2210);

Console.WriteLine($"{metrics.CurrentTokens}/{metrics.ContextWindowTokens} tokens used; compact at {metrics.CompactThresholdTokens} ({metrics.CompactThresholdRatio:P0})");
```

## Notes
- ConversationTokens excludes the rolling summary; the summary token count is held in SummaryTokens and IsSummarized indicates whether a rolling summary is present.
- ProjectPromptTokens and MemoryTokens are zero when no project override or memories exist.
- ToolsTokens represents descriptors (name, description, schema) billed against the same context window even though they are not part of the messages array.
- The numeric totals are intended to match the provider-facing assembly logic; small discrepancies can occur if provider-specific per-message overhead changes.
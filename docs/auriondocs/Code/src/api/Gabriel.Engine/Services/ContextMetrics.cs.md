# ContextMetrics

> **File:** `src/api/Gabriel.Engine/Services/ContextMetrics.cs`  
> **Kind:** record

A snapshot of token usage and compaction state that the agent service evaluates immediately before sending the next turn. Use this when you need the same token accounting and compact/threshold decision the backend uses (for example to show a "context used / until compact" indicator in the UI that matches server-side behavior).

## Remarks
This record exposes a per-category breakdown of how tokens are consumed (system prompts, project prompt, memories, tools, conversation messages and an optional rolling summary) plus the context window and compaction thresholds the agent uses to decide whether to compact history. The breakdown mirrors what ToProviderHistory assembles so a UI grid or progress meter can add the categories up and match the backend's actual decision process.

## Example
```csharp
// Typical usage: ask the agent service for current metrics and render a progress bar
var metrics = await agentService.GetContextMetricsAsync(conversationId);
var used = metrics.CurrentTokens;
var capacity = metrics.ContextWindowTokens;
var percent = capacity == 0 ? 0 : (double)used / capacity;
Console.WriteLine($"Tokens: {used}/{capacity} ({percent:P1})");
Console.WriteLine($"Compact threshold: {metrics.CompactThresholdTokens} tokens ({metrics.CompactThresholdRatio:P0})");
if (metrics.IsSummarized)
{
    Console.WriteLine($"Summary present: {metrics.SummaryTokens} tokens");
}
```

## Notes
- ProjectPromptTokens and MemoryTokens are zero when the project has no override or no saved memories respectively.
- ToolsTokens are counted against the same context window even though tool descriptors are not part of the message array.
- The per-category fields are intended to sum to CurrentTokens up to a small per-message overhead; do not assume large discrepancies without checking provider-specific accounting.
- Use CompactThresholdTokens/CompactThresholdRatio to show when the backend is likely to compact; the agent service's internal MaybeCompactAsync performs the authoritative check.
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


ContextMetrics is a C# record that captures a snapshot of the token budget and contextual framing for the next agent turn. It is surfaced by the backend via IAgentService.GetContextMetricsAsync so the UI can render indicators like a context usage gauge and the point at which the context would be compacted, ensuring the frontend view matches the backend's decision to trim, summarize, or adjust the context.

The fields provide a categorized breakdown of token usage (system prompts, memory, tools, and the per-turn conversation content) that, together with the total CurrentTokens, reflect how the upcoming message set will fit within the context window. The per-category breakdown mirrors what ToProviderHistory assembles, enabling the UI grid to sum to CurrentTokens and stay aligned with the backend's accounting.

## Remarks
ContextMetrics acts as a bridge between the agent's internal token-budget model and the user-facing indicators. It decouples the cost calculus from rendering, so UI components can react to changes in summarization, memory usage, or prompt overrides without re-deriving the entire history. The breakdown helps diagnose which parts of the prompt and conversation contribute most to the current token footprint, facilitating informed tuning by developers and operators.

## Example
```csharp
var metrics = new ContextMetrics(
    CurrentTokens: 1200,
    ContextWindowTokens: 1500,
    CompactThresholdTokens: 1000,
    CompactThresholdRatio: 0.75,
    MessagesAfterCut: 2,
    IsSummarized: false,
    SummaryTokens: 200,
    SystemPromptTokens: 320,
    ProjectPromptTokens: 0,
    MemoryTokens: 150,
    ToolsTokens: 60,
    ConversationTokens: 980);
```

## Notes
- ContextMetrics represents a point-in-time snapshot for the upcoming turn; changes to user input or prompts will produce new metrics.
- The per-category fields should align with the total CurrentTokens and reflect the same conceptual partitions used by the provider history and UI visualizations. When a project-level prompt override or memories are in effect, the corresponding tokens will reflect those contributions.

# AgentContext.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentContext.cs`

## Contents

- [AgentContext](#agentcontext)
- [AgentContextBreakdown](#agentcontextbreakdown)

---

## AgentContext
> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

```csharp
public record AgentContext(
    string PersonaPrompt,
    string? ProjectPrompt,
    string? MemoryBlock,
    string? Summary,
    IReadOnlyList<Message> Messages,
    IReadOnlyList<ToolDescriptor> Tools)
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `PersonaPrompt` | `string` | — |
| `ProjectPrompt` | `string?` | — |
| `MemoryBlock` | `string?` | — |
| `Summary` | `string?` | — |
| `Messages` | `IReadOnlyList<Message>` | — |
| `Tools` | `IReadOnlyList<ToolDescriptor>` | — |


AgentContext is a C# record that captures all inputs and derived messages that are sent to the chat provider for a single turn. It serves as the single source of truth for what a turn sends to the provider by consolidating the PersonaPrompt, optional ProjectPrompt, MemoryBlock, Summary, the Messages being considered, and the Tools into a stable value object. The static Build method constructs an AgentContext from a Conversation and the prompts and tooling prepared for this turn, performing variant filtering and orphaned-tool-message cleanup so callers don’t have to reimplement that logic. It also collects the set of active tool_call.ids referenced by active assistant messages and filters tool messages so only relevant tool results survive, ensuring live interactions and metrics observe identical data. ToProviderHistory then materializes the exact provider-facing message sequence by prefixing system messages in a fixed order: persona, optional project context, saved memories, rolling summary, and finally the filtered conversation, guaranteeing deterministic model behavior and parity between the live call and any UI-based metrics.

---

## AgentContextBreakdown
> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

```csharp
public record AgentContextBreakdown(
    int SystemPromptTokens,
    int ProjectPromptTokens,
    int MemoryTokens,
    int SummaryTokens,
    int ToolsTokens,
    int ConversationTokens)
{
    public int Total =>
        SystemPromptTokens + ProjectPromptTokens + MemoryTokens
        + SummaryTokens + ToolsTokens + ConversationTokens;
}
```

**Parameters:**

| Parameter | Type | Default |
|-----------|------|---------|
| `SystemPromptTokens` | `int` | — |
| `ProjectPromptTokens` | `int` | — |
| `MemoryTokens` | `int` | — |
| `SummaryTokens` | `int` | — |
| `ToolsTokens` | `int` | — |
| `ConversationTokens` | `int` | — |


AgentContextBreakdown is a record that captures per-category token totals from a single AgentContext snapshot. It exposes six token counters (SystemPromptTokens, ProjectPromptTokens, MemoryTokens, SummaryTokens, ToolsTokens, ConversationTokens) and a computed Total that aggregates them. This breakdown is used by the AgentService during its compact decision and is surfaced publicly as ContextMetrics.CurrentTokens, enabling callers to reason about overall and category-specific token usage without recomputing sums.

## Remarks
The abstraction decouples token accounting from the decision logic, providing a stable, immutable view of token usage that can be passed across components, logged, or displayed in diagnostics. If new token categories are needed in the future, the record can be extended without altering consumer code that relies on the existing shape.

## Notes
- Be aware of potential integer overflow if token counts ever approach 2,147,483,647; in practice, counts are far smaller.

---
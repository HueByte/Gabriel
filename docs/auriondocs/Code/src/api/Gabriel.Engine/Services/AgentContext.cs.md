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


AgentContext is a record that encapsulates the complete context sent to the chat provider for a single turn. It centralizes persona context, optional project context, memory block, a summary, the active messages, and tool descriptors into a single, immutable value so both the live provider call and any telemetry built for the UI share the exact same data.

## Remarks

AgentContext serves as the canonical snapshot of a turn’s context used by both runtime interactions with the provider and by the UI/metrics layer. By performing variant filtering and orphaned-tool-message cleanup in one place, it prevents divergence between what the model actually sees and what is exposed in telemetry, and it avoids reassembly inconsistencies between the live call path and the metrics path.

## Example

```csharp
// Example usage
Conversation conversation = /* existing conversation */;
string personaPrompt = /* persona */;
string? projectPrompt = /* project context */;
string? memoryBlock = /* memory block */;
IReadOnlyList<ToolDescriptor> tools = /* tool descriptors */;

var context = AgentContext.Build(conversation, personaPrompt, projectPrompt, memoryBlock, tools);
var providerHistory = context.ToProviderHistory();
```

## Notes

- Build starts from the conversation’s messages, optionally skipping messages before a summarized point. If SummarizedThroughMessageId is unset, it includes all messages from the start.
- It collects the set of active tool_call.ids from active assistant messages and filters tool messages to only those with an active tool call; non-tool messages are kept only if they are part of an active variant. Malformed or missing tool call data can influence what gets retained.


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


AgentContextBreakdown is an immutable data carrier that captures per-category token counts for a single AgentContext snapshot. It corresponds to the token breakdown used by the AgentService when deciding on compact behavior and provides a Total that sums all categories, matching the value represented by ContextMetrics.CurrentTokens.

## Remarks
AgentContextBreakdown serves as a focused, transport-friendly representation of token usage. It aggregates the six categories into one object, making it easier to log, compare, or feed into decision logic without pulling the full AgentContext. The Total property offers a single, convenient aggregate that aligns with the token tally used by AgentService and with ContextMetrics.

## Notes
- Total is computed from six fields; since the record is immutable, the value is fixed once constructed. The sum uses unchecked integer arithmetic; values approaching int.MaxValue could overflow; if there is a risk, consider using a larger type or introducing validation.


---
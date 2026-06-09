# AgentContext.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentContext.cs`

## Contents

- [AgentContext](#agentcontext)
- [AgentContextBreakdown](#agentcontextbreakdown)

---

## AgentContext

> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

A compact, immutable value object that represents everything a single agent turn will send to the chat provider: persona and project prompts, an optional memory block and rolling summary, the filtered conversation messages for the turn, and the available tool descriptors. Use AgentContext when you need a single, consistent assembly of prompts/messages/tools to drive a provider call or to compute UI metrics so both consumers see identical inputs.

## Remarks
AgentContext centralizes the logic that used to be duplicated inside AgentService so the live provider call and the metrics/summary calculations remain in sync. The Build factory handles variant filtering, removes messages prior to the conversation summary cut, and prunes tool messages that are orphaned (i.e., tool results not referenced by any active assistant message). ToProviderHistory then emits system messages in a stable order (persona, optional project context, optional memory block, rolling summary) before the conversation messages — preserving previous model-facing ordering to avoid behavioral regressions.

## Example
```csharp
// Assemble an AgentContext for the current turn, then get the provider history
var ctx = AgentContext.Build(
    conversation: conversation,
    personaPrompt: personaPrompt,
    projectPrompt: projectPrompt,
    memoryBlock: memoryBlock,
    tools: toolDescriptors);

IReadOnlyList<ChatProviderMessage> providerHistory = ctx.ToProviderHistory();
// providerHistory can now be passed to the chat provider or used for metrics.
```

## Notes
- Build removes messages up to conversation.SummarizedThroughMessageId (if present) so the returned Messages list starts after the summary cut.
- Tool-role messages are retained only when their ToolCallId is referenced by an active assistant message in this turn; legacy or orphaned tool messages are dropped.
- ProjectPrompt, MemoryBlock and Summary may be null/empty; ToProviderHistory only prepends the corresponding system blocks when they contain content.
- The factory parses assistant ToolCallsJson to collect referenced tool_call ids — malformed JSON here would raise during Build, so inputs should be well-formed.
- AgentContext is immutable (record); callers should construct a new instance via Build rather than mutating an existing one.


---

## AgentContextBreakdown

> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

Represents a per-category token count snapshot for an AgentContext. Use this record when you need a compact, read-only view of how many tokens each part of the context (system prompt, project prompt, memory, summary, tools, conversation) is consuming or when you need the aggregated total as used by AgentService and exposed via ContextMetrics.CurrentTokens.

## Remarks
This is a lightweight, immutable DTO that captures token usage at a single point in time. It mirrors the breakdown AgentService uses for its compact decision logic, making it suitable for telemetry, diagnostics, or enforcing token-related limits without querying the live context.

## Example
```csharp
// Create a snapshot with per-category token counts
var breakdown = new AgentContextBreakdown(
    SystemPromptTokens: 120,
    ProjectPromptTokens: 80,
    MemoryTokens: 300,
    SummaryTokens: 50,
    ToolsTokens: 10,
    ConversationTokens: 200);

Console.WriteLine($"Total tokens: {breakdown.Total}"); // Total tokens: 760
```

## Notes
- The record is immutable (positional properties), so instances represent a snapshot rather than a live-updating view. 
- No validation is performed on the inputs; very large sums could overflow an int (rare for typical token counts).

---
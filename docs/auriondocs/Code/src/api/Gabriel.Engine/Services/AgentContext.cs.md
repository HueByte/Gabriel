# AgentContext.cs

> **Source:** `src/api/Gabriel.Engine/Services/AgentContext.cs`

## Contents

- [AgentContext](#agentcontext)
- [AgentContextBreakdown](#agentcontextbreakdown)

---

## AgentContext

> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

An immutable value object that represents everything a single turn sends to the chat provider: persona prompt, optional project prompt, optional memory block, optional rolling summary, the filtered conversation messages, and the set of available tool descriptors. Use AgentContext when you need a single, canonical assembly of the data sent to the provider (for making the provider call or for presenting consistent UI metrics) rather than assembling those pieces in multiple places.

## Remarks
AgentContext centralizes the logic that previously lived in multiple places so the live provider call and any UI metrics or diagnostics observe exactly the same inputs. It performs conversation trimming (honoring Conversation.SummarizedThroughMessageId), removes inactive message variants, and prunes tool messages that are not referenced by active assistant tool_call entries — ensuring legacy or orphaned tool outputs do not resurface. The record is intentionally immutable and exposes read-only collections so callers treat the assembled context as a single source of truth.

## Example
```csharp
// Build an AgentContext for the current turn and produce the provider-ready history
var ctx = AgentContext.Build(conversation, personaPrompt, projectPrompt, memoryBlock, tools);
var providerHistory = ctx.ToProviderHistory();
// providerHistory can now be passed to the chat provider client
```

## Notes
- Only active variants are kept; messages with IsActiveVariant == false are excluded from Messages.
- Tool role messages are retained only when their ToolCallId is referenced by an active assistant message's tool calls; otherwise they are dropped to avoid showing orphaned tool results.
- The provider history prepends system messages in a specific order (persona, project context, saved memories, rolling summary) before the filtered conversation so model behaviour remains consistent with prior assembly logic.

---

## AgentContextBreakdown

> **File:** `src/api/Gabriel.Engine/Services/AgentContext.cs`  
> **Kind:** record

Represents a per-category token count snapshot taken from an AgentContext. Use this record when you need a stable breakdown of how many tokens each part of an agent context (system prompt, project prompt, memory, summary, tools, conversation) consumes — for example when reporting ContextMetrics.CurrentTokens or when making compaction/decision logic that depends on the same total used by AgentService.

## Remarks
This is a small immutable DTO that groups the individual token categories together and exposes a computed Total that is the exact sum of the fields. It exists to keep token-category counts consistent across metrics and decision logic so callers can rely on a single source of truth for both per-category counts and the overall token total.

## Example
```csharp
var breakdown = new AgentContextBreakdown(
    SystemPromptTokens: 120,
    ProjectPromptTokens: 80,
    MemoryTokens: 200,
    SummaryTokens: 40,
    ToolsTokens: 10,
    ConversationTokens: 350);

int total = breakdown.Total; // 800
```

## Notes
- This is a snapshot: the record captures counts at a moment in time and does not update if the underlying context changes.
- The constructor does not validate values (e.g. negative counts); callers should ensure inputs are sensible.
- Total is computed as an int by summing fields; extremely large counts could overflow an int (unlikely in normal token usage).

---
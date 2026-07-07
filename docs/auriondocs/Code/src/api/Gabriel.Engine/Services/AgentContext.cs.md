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


AgentContext is a record that encapsulates all the context the chat provider needs for a single turn. It combines the agent persona, an optional project prompt, an optional memory block, the turn summary, the filtered sequence of messages, and the available tool descriptors into a single, immutable object used by both the live provider path and the UI metrics path. Build constructs this context from a Conversation and the prompt fragments, applying filtering and orphaned-tool-message cleanup so callers always operate on a consistent view.

## Remarks
Architecturally, this abstraction centralizes the logic for what is shared between the actual provider invocation and the metrics UI. Previously, two inline assemblies diverged; merging them into AgentContext ensures tool results tied to active tool calls are preserved across both paths and that inactive or orphaned messages are dropped early. It also isolates the transformation from high-level prompt fragments to a provider-ready history, simplifying future changes to how prompts are composed.

## Example
```csharp
// Most common usage
var context = AgentContext.Build(conversation, personaPrompt, projectPrompt, memoryBlock, tools);
var history = context.ToProviderHistory();
```

## Notes
- Build trims messages using SummarizedThroughMessageId if present; if the summary marker cannot be found, no trimming occurs (startIdx remains 0).
- Only active variant messages are kept; tool messages are retained only if their ToolCallId appears in the set of active tool_call IDs gathered from active assistant messages.
- ProjectPrompt is optional; when it is null or whitespace, the corresponding system block is omitted from the provider history.


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


Documentation submitted for symbol AgentContextBreakdown. The narrative covers its purpose, rationale, and practical notes; no example block included.

---